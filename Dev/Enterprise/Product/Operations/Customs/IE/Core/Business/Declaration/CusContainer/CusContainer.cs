using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CusContainer : EU.Business.Declaration.CusContainer
		, Integration.Customs.IE.ICusContainer
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("IE.CusContainer.CO_ContainerNumber|UCC5", Caption = "[7/10] Container Number", FullDescription = "[7/10] Container identification number", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		[ResourceStringData("543D5CEC-4137-486C-9BDD-C61CF8948773", Caption = "[7/10] Container Number", MediumCaption = "Container Num.", ShortCaption = "Cont. Num.")]
		public override ZString CO_ContainerNumber
		{
			get { return base.CO_ContainerNumber; }
			set { base.CO_ContainerNumber = value; }
		}

		[ResourceStringData("IE.CusContainer.ContainerNumberForBinding|UCC5", Caption = "[7/10] Container Number", FullDescription = "[7/10] Container identification number", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		[ResourceStringData("FF45394B-425E-4091-8F55-CC535DE27708", Caption = "[7/10] Container Number", MediumCaption = "Container Num.", ShortCaption = "Cont. Num.")]
		public override ZString ContainerNumberForBinding
		{
			get { return CO_ContainerNumber; }
			set { CO_ContainerNumber = value; }
		}

		[ResourceStringData("IE.CusContainer.CO_Seal|UCC5", Caption = "[7/18] Seal Number", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString CO_Seal
		{
			get { return base.CO_Seal; }
			set { base.CO_Seal = value; }
		}

		[ResourceStringData("IE.CusContainer.SealNumberForBinding|UCC5", Caption = "[7/18] Seal Number", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString SealNumberForBinding
		{
			get { return base.SealNumberForBinding; }
			set { base.SealNumberForBinding = value; }
		}
	}
}
