using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class DeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasureWrapper : IDeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasure
	{
		public DeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasureWrapper(CusEntryInstruction entryInstruction, JobDeclaration jobDeclaration)
		{
			this.entryInstruction = entryInstruction;
			this.jobDeclaration = jobDeclaration;
		}

		public static DeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasureWrapper NewOrNull(CusEntryInstruction entryInstruction, JobDeclaration jobDeclaration)
			=> entryInstruction != null && jobDeclaration != null ? new DeclarationGoodsShipmentConsignmentDmExtensionsPackagesMeasureWrapper(entryInstruction, jobDeclaration) : null;

		public IMeasureType GrossMassMeasure => MeasureTypeWrapper.NewOrNull(jobDeclaration.JE_TotalWeight, jobDeclaration.JE_TotalWeightUnit);

		public ITextType MarksNumbers => null;

		public ICodeType PackageMeasureQualifier => CodeTypeWrapper.NewOrNull("2");

		public IQuantityType TotalPackageQuantity => QuantityTypeWrapper.NewOrNull((ZDecimal)entryInstruction.CEI_NumberOfPackages, ZString.Empty);

		public ICodeType TypeCode => CodeTypeWrapper.NewOrNull(entryInstruction.CEI_CustomsPackType);

		readonly JobDeclaration jobDeclaration;
		readonly CusEntryInstruction entryInstruction;
	}
}
