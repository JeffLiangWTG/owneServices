using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class InwardProcessingPlaceCollection : DependentBusinessObjectCollection<InwardProcessingPlace, CusEntryInstruction>
	{
		const int MaxCountForValidation = 999;

		public InwardProcessingPlaceCollection(CusEntryInstruction master) : base(master, new ZQuery(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.InwardProcessingPlace))
		{
			MaxCountValidationEnable(MaxCountForValidation);
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK) => typeof(InwardProcessingPlace);

		protected override SchemaGuidColumn FKSchemaColumnInDependent => JobDocAddressSchema.E2_ParentID;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var inwardProcessingPlace = (InwardProcessingPlace)child;
			inwardProcessingPlace.E2_AddressType = DocAddressTypes.Codes.InwardProcessingPlace;
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			var inwardProcessingPlace = bizO as InwardProcessingPlace;
			inwardProcessingPlace?.Instruction?.SequenceGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(inwardProcessingPlace);
		}
	}
}
