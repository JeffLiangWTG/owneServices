using CargoWise.ComponentModel;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSAddInfoJobDeclaration : AddInfo
	{
		public EMCSAddInfoJobDeclaration(EMCSJobDeclaration declaration)
			: base(declaration.JE_AddInfoInfo)
		{
			Declaration = declaration;
		}
		protected readonly EMCSJobDeclaration Declaration;

		[List(nameof(Lookups) + "." + nameof(EMCSAddInfoJobDeclarationLookups.DeferredSubmissionList))]
		public override ZString ZG_DeferredSubmission
		{
			get => base.ZG_DeferredSubmission;
			set => base.ZG_DeferredSubmission = value;
		}

		[List(nameof(Lookups) + "." + nameof(EMCSAddInfoJobDeclarationLookups.GuarantorTypeList))]
		public override ZString ZG_GuarantorType
		{
			get => base.ZG_GuarantorType;
			set => base.ZG_GuarantorType = value;
		}

		[List(nameof(Lookups) + "." + nameof(EMCSAddInfoJobDeclarationLookups.TransportArrangementList))]
		public override ZString ZG_TransportArrangement
		{
			get => base.ZG_TransportArrangement;
			set => base.ZG_TransportArrangement = value;
		}

		[List(nameof(Lookups) + "." + nameof(EMCSAddInfoJobDeclarationLookups.OriginTypeList))]
		public override ZString ZG_OriginType
		{
			get => base.ZG_OriginType;
			set => base.ZG_OriginType = value;
		}

		[List(nameof(Lookups) + "." + nameof(EMCSAddInfoJobDeclarationLookups.SubmissionTypeList))]
		public override ZString ZG_SubmissionType
		{
			get => base.ZG_SubmissionType;
			set => base.ZG_SubmissionType = value;
		}

		[List(nameof(Lookups) + "." + nameof(EMCSAddInfoJobDeclarationLookups.EuropeanUnionCountryList))]
		public override ZString ZG_CCTMSA
		{
			get => base.ZG_CCTMSA;
			set => base.ZG_CCTMSA = value;
		}

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get
			{
				return new SchemaColumn[] {
					EUEMCSAddInfoSchema.ZG_DeferredSubmission,
					EUEMCSAddInfoSchema.ZG_GuarantorType,
					EUEMCSAddInfoSchema.ZG_OriginType,
					EUEMCSAddInfoSchema.ZG_TransportArrangement
				};
			}
		}

		public new EMCSAddInfoJobDeclarationValidation Validation => (EMCSAddInfoJobDeclarationValidation)base.Validation;
		protected override EUEMCSAddInfoValidation GetNewValidation() => new EMCSAddInfoJobDeclarationValidation(this);

		public new EMCSAddInfoJobDeclarationLookups Lookups => (EMCSAddInfoJobDeclarationLookups)base.Lookups;
		protected override EUEMCSAddInfoLookups GetNewLookups() => new EMCSAddInfoJobDeclarationLookups(this);
	}
}
