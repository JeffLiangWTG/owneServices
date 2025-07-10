using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.CACCN)]
	public class CACargoControlNumberAddInfo : AutoCACargoControlNumberAddInfo
	{
		public CACargoControlNumberAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			Parent = addInfoProperty.BizObj;
			SetupEventsAndLoadValues(addInfoProperty);
		}

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get
			{
				return new SchemaColumn[] {
					CACargoControlNumberAddInfoSchema.CA_CCNInfoNumber
				};
			}
		}

		public override ZString CA_CCNInfoNumber
		{
			get => base.CA_CCNInfoNumber;
			set
			{
				bool hasChanges = base.CA_CCNInfoNumber != value;
				base.CA_CCNInfoNumber = value;
				if (hasChanges)
				{
					MarkParentAsNeedingValidation();
				}
			}
		}

		public override ZBool CA_IsFromNumbersTab
		{
			get => base.CA_IsFromNumbersTab;
			set
			{
				bool hasChanges = base.CA_IsFromNumbersTab != value;
				base.CA_IsFromNumbersTab = value;
				if (hasChanges)
				{
					MarkParentAsNeedingValidation();
				}
			}
		}

		void MarkParentAsNeedingValidation()
		{
			if (Parent != null)
			{
				var cargocontrolNumber = Parent as CargoControlNumber;
				if (cargocontrolNumber != null && cargocontrolNumber.Parent != null && !cargocontrolNumber.Parent.IsMarkingAsNeedingValidationSuspended)
				{
					cargocontrolNumber.Parent.MarkAsNeedingValidation();
				}
			}
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (HasChanges && Parent != null && !Parent.IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}
	}
}
