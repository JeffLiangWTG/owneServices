using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public abstract class CusCodeDataWithSequenceNumberLine : CusCodeData,
		IShortSequenceNumberLine
	{
		protected CusCodeDataWithSequenceNumberLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString CY_Type
		{
			get { return base.CY_Type; }
			set
			{
				if (value != Type)
				{
					ErrorReporter.ReportOnce(GetType().FullName + ".CY_Type Invalid Setting", "CY_Type should be '" + Type + "'");
				}
				ZString oldValue = CY_Type;
				base.CY_Type = value;
				if (!IsCopying && oldValue != CY_Type)
				{
					MarkParentAsNeedingValidation();
				}
			}
		}

		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set
			{
				ZString oldValue = CY_Data;
				base.CY_Data = value;
				if (!IsCopying && oldValue != CY_Data)
				{
					MarkParentAsNeedingValidation();
				}
			}
		}

		public override ZGuid CY_ParentID
		{
			get { return base.CY_ParentID; }
			set
			{
				var oldHeader = Parent;
				ZGuid oldValue = CY_ParentID;
				base.CY_ParentID = value;
				if (!IsCopying && oldValue != CY_ParentID)
				{
					SetOrderOnSettingCY_ParentID(oldHeader);
					MarkParentAsNeedingValidation();
				}
			}
		}

		public override ZString CY_ParentTableCode
		{
			get { return base.CY_ParentTableCode; }
			set
			{
				ZString oldValue = CY_ParentTableCode;
				base.CY_ParentTableCode = value;
				if (!IsCopying && oldValue != CY_ParentTableCode)
				{
					MarkParentAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("CusCodeDataWithSequenceNumberLine|CY_Order", Caption = "Order")]
		[BusinessObjectTestExclude]
		public override ZShort CY_Order
		{
			get { return base.CY_Order; }
			set
			{
				if (value > 0)
				{
					ZShort oldValue = CY_Order;

					base.CY_Order = value;

					var header = Parent;
					if (!IsCopying && header != null)
					{
						GetSequenceNumberGenerator(header).RecalculateWhenRenumbered(this, oldValue);
					}
				}
			}
		}

		public override void Delete()
		{
			var header = Parent;
			if (header != null)
			{
				GetSequenceNumberGenerator(header).RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
			base.Delete();
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JPAFRBills)); }
		}

		#region Implementation

		void SetOrderOnSettingCY_ParentID(BusinessObject oldParent)
		{
			if (!IsCopying)
			{
				if (oldParent != null)
				{
					GetSequenceNumberGenerator(oldParent).RecalculateWhenAboutToBeDetachedOrDeleted(this);
				}

				var header = Parent;
				if (header != null)
				{
					GetSequenceNumberGenerator(header).RecalculateWhenAdded(this);
				}
			}
		}

		protected abstract ShortSequenceNumberGenerator GetSequenceNumberGenerator(BusinessObject bizObj);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = Type;
		}

		protected abstract string Type { get; }

		#endregion

		#region IShortSequenceNumberLine Members

		ZGuid ISequenceNumberLine.FKToHeader => CY_ParentID;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => CY_Order;
			set => CY_Order = value;
		}

		#endregion
	}
}
