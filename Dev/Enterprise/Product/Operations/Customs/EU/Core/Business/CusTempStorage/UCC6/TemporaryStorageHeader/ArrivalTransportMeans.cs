using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class ArrivalTransportMeans : CusTransportMeans
	{
		public ArrivalTransportMeans(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusTransportMeansValidation GetNewValidation() => new ArrivalTransportMeansValidation(this);

		public new ArrivalTransportMeansValidation Validation => (ArrivalTransportMeansValidation)base.Validation;

		public TemporaryStorageHeader ParentHeader => parentHeader ??= Factory.Load<TemporaryStorageHeader>(this.TPM_ParentID);

		TemporaryStorageHeader parentHeader;

		[ResourceStringData("c0c9bf9f-ac8a-44db-880a-eb1f62f08d5b", Caption = "Arrival Transport Means")]
		public override ZString TPM_IdentificationNumber
		{
			get => base.TPM_IdentificationNumber;
			set => base.TPM_IdentificationNumber = value;
		}

		public override ZGuid TPM_ParentID
		{
			get => base.TPM_ParentID;
			set
			{
				var oldValue = TPM_ParentID;
				base.TPM_ParentID = value;
				if (!IsCopying && oldValue != value)
				{
					ParentHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString TPM_ParentTableCode
		{
			get => base.TPM_ParentTableCode;
			set
			{
				var oldValue = TPM_ParentTableCode;
				base.TPM_ParentTableCode = value;
				if (!IsCopying && oldValue != value)
				{
					ParentHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString TPM_TypeOfIdentification
		{
			get => base.TPM_TypeOfIdentification;
			set
			{
				var oldValue = TPM_TypeOfIdentification;
				base.TPM_TypeOfIdentification = value;
				if (!IsCopying && oldValue != value)
				{
					ParentHeader?.MarkAsNeedingValidation();
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TPM_ParentTableCode = "AMA";
		}
	}
}
