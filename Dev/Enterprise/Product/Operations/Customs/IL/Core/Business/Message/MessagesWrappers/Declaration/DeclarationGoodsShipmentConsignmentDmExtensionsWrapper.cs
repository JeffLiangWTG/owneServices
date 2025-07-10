using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentConsignmentDmExtensionsWrapper : IDeclarationGoodsShipmentConsignmentDmExtensions
	{
		DeclarationGoodsShipmentConsignmentDmExtensionsWrapper(CusEntryInstruction entryInstruction, JobDeclaration jobDeclaration)
		{
			this.entryInstruction = entryInstruction;
			this.jobDeclaration = jobDeclaration;
		}

		public static DeclarationGoodsShipmentConsignmentDmExtensionsWrapper NewOrNull(CusEntryInstruction entryInstruction, JobDeclaration jobDeclaration)
			=> entryInstruction != null && jobDeclaration != null ? new DeclarationGoodsShipmentConsignmentDmExtensionsWrapper(entryInstruction, jobDeclaration) : null;

		public ITextType CargoDescription => null;

		public ICodeType ExportationCountryCode => CodeTypeWrapper.NewOrNull(jobDeclaration.JE_GoodsOrigin);

		public ILastReleaseFromWarehousIndType LastReleaseFromWarehousInd => null;

		public ICollection<IDeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasure> PackagesMeasure
		{
			get
			{
				var collection = new Collection<IDeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasure>();

				collection.Add(DeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasureWrapper.NewOrNull(entryInstruction, jobDeclaration));
				
				return collection;
			}
		}

		public ICollection<IDeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacility> RegisteredFacility
		{
			get
			{
				var collection = new Collection<IDeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacility>();

				collection.Add(DeclarationGoodsShipmentConsignmentDmExtensionsRegisteredFacilityStorageSiteWrapper.NewOrNull(jobDeclaration));

				return collection;
			}
		}

		readonly JobDeclaration jobDeclaration;
		readonly CusEntryInstruction entryInstruction;
	}
}
