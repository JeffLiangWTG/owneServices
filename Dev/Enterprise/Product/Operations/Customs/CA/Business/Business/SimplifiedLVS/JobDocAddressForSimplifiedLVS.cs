using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	class JobDocAddressForSimplifiedLVS : JobDocAddress
	{
		public JobDocAddressForSimplifiedLVS(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static JobDocAddressForSimplifiedLVS New(SimplifiedLVS simplifiedLVS, ZString addressType)
		{
			var result = simplifiedLVS.Factory.New<JobDocAddressForSimplifiedLVS>();
			result.AdditionalValidation = new SimplifiedLVSJobDocAddressValidation(simplifiedLVS, result);
			result.SimplifiedLVS = simplifiedLVS;
			result.E2_AddressType = addressType;
			result.DefaultAddressType = ZArchitecture.Business.AddressType.NoDefault;
			return result;
		}

		public SimplifiedLVS SimplifiedLVS
		{
			get { return fSimplifiedLVS; }
			private set
			{
				fSimplifiedLVS = value;
				E2_ParentID = fSimplifiedLVS.PK;
				fSimplifiedLVS.RegisterEditableChildObject(this);
			}
		}
		SimplifiedLVS fSimplifiedLVS;

		protected override JobDocAddressValidation GetNewValidation()
		{
			var validation = base.GetNewValidation();
			var validationInternals = (IValidationInternals)validation;
			validationInternals.Add(AdditionalValidation);
			return validation;
		}

		public override bool ReadOnly
		{
			get { return SimplifiedLVS.ReadOnly; }
			set { }
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}
	}
}
