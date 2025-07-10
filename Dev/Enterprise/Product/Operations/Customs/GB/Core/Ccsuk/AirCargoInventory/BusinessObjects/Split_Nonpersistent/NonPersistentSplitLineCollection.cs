using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentSplitLineCollection : NonPersistentBusinessObjectCollection<NonPersistentSplitLine>
	{
		public NonPersistentSplitLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			MaxCountValidationEnable(maxRows);
		}

		public NonPersistentSplitLineCollection(ICcsukCusAwb awb)
			: this(awb.Factory)
		{
			this.awb = awb;
		}
		readonly ICcsukCusAwb awb;

		public bool IsAllocatingNprWithFlightData
		{
			get;
			set;
		}

		protected override bool AllowNewCore
		{
			get { return Count < maxRows && !IsAllocatingNprWithFlightData; }
		}

		protected override bool AllowRemoveCore
		{
			get { return !IsAllocatingNprWithFlightData; }
		}

		protected override BusinessObject AddNewCore()
		{
			var bo = CreateNonPersistentBusinessObject();
			var split = (NonPersistentSplitLine)bo;
			split.ParentCollection = this;
			base.Add(split);
			return bo;
		}

		public override void Add(BusinessObject businessObject)
		{
			var split = (NonPersistentSplitLine)businessObject;
			split.ParentCollection = this;
			base.Add(split);
			if (!split.SplitNumber.IsEmpty)
			{
				var thisSplitNumber = ZInt.Parse(split.SplitNumber);
				if (thisSplitNumber > highestSplitNumberSoFar)
				{
					highestSplitNumberSoFar = thisSplitNumber;
				}
			}
			else
			{
				highestSplitNumberSoFar++;
				if (highestSplitNumberSoFar.ToString().Length > NonPersistentSplitLine.Schema.SplitNumberMaxLength)
				{
					ErrorReporter.ReportOnce("BGB-NonPersistentSplitLine-TooManyRows", "AllowNewCore should limit the number of splits to 99, but a bug in Core means that sometimes one can add too many rows.  This then violates the maxlength of a 2-char field");
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			NonPersistentSplitLine newLine = null;
			highestSplitNumberSoFar++;
			if (awb == null)
			{
				newLine = new NonPersistentSplitLine(highestSplitNumberSoFar, Factory);
			}
			else
			{
				newLine = new NonPersistentSplitLine(highestSplitNumberSoFar, awb);
			}
			newLine.ParentCollection = this;
			return newLine;
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			// For when user removes his cursor from an empty row after changing his mind about adding a new row. 
			var split = (NonPersistentSplitLine)elementToRemove;
			base.Remove(split);
			DecreaseSplitCount(split);
		}

		void DecreaseSplitCount(NonPersistentSplitLine splitToRemoveOrDelete)
		{
			if (!splitToRemoveOrDelete.SplitNumber.IsEmpty)
			{
				var thisSplitNumber = ZInt.Parse(splitToRemoveOrDelete.SplitNumber);
				if (thisSplitNumber == highestSplitNumberSoFar)
				{
					highestSplitNumberSoFar--;
				}
			}
			splitToRemoveOrDelete.IsDeletingOrRemovingFromCollection = true;
			var anySplit = (NonPersistentSplitLine)this.Except(splitToRemoveOrDelete).FirstOrDefault();
			if (anySplit != null)
			{
				anySplit.Validation.ValidateSplitNumber();
			}
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			base.RemoveCollectionRelationshipsCore(child, forDelete);
			DecreaseSplitCount((NonPersistentSplitLine)child);
		}

		public ZString FormatForGenral()
		{
			var sb = new ZStringBuilder();
			foreach (NonPersistentSplitLine line in this)
			{
				sb.Append(line.FormatForGenral());
			}
			return sb.ToStringWithNewLineBetweenAppends();
		}
		ZInt highestSplitNumberSoFar = 0;

#if DEBUG
		protected virtual
#endif
		int maxRows
		{ get { return 99; } }
	}
}
