using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusSeal : Customs.Business.CusSeal, IShortSequenceNumberLine
	{
		public CusSeal(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		BusinessObject fParent;
		public BusinessObject Parent
		{
			get
			{
				if (fParent == null && !BK_ParentTableCode.IsEmpty && !BK_ParentID.IsEmpty)
				{
					fParent = Factory.Load(BK_ParentTableCode, BK_ParentID);
				}
				return fParent;
			}
		}

		public override ZString BK_ParentTableCode
		{
			get => base.BK_ParentTableCode;
			set
			{
				var oldValue = BK_ParentTableCode;
				base.BK_ParentTableCode = value;

				if (!IsCopying && oldValue != value)
				{
					SequenceNumberGenerator?.RecalculateWhenAdded(this);
				}
			}
		}

		[ResourceStringData("11984707-A48A-492A-A667-5B060E5281BF", Caption = "Seal Number")]
		public override ZString BK_SealNumber { get => base.BK_SealNumber; set => base.BK_SealNumber = value; }

		public override void Delete()
		{
			if (!IsDeleted)
			{
				SequenceNumberGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
			base.Delete();
		}

		#region Sequence Number

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber { get => BK_SequenceNumber; set => BK_SequenceNumber = value; }

		ZGuid ISequenceNumberLine.FKToHeader => BK_ParentID;

		ShortSequenceNumberGenerator SequenceNumberGenerator => (Parent as ICusSealSequenceNumberGeneratorProvider)?.SequenceNumberGenerator;

		#endregion

		protected override bool SupportsCloneCore() => true;
	}
}
