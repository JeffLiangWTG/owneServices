using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class JobDeclaration
	{
		[ResourceStringData("IE.AddInfo.ZG_PresentationStartDate", Caption = "Start", FullDescription = "Date and Time of Presentation of the Goods - 15 08 001 000")]
		public override ZDateTime ZG_PresentationStartDate { get => base.ZG_PresentationStartDate; set => base.ZG_PresentationStartDate = value; }

		public override ZString ZG_SpecificCircumstanceIndicator
		{
			get => base.ZG_SpecificCircumstanceIndicator;
			set
			{
				var oldValue = ZG_SpecificCircumstanceIndicator;
				base.ZG_SpecificCircumstanceIndicator = value;
				if (!IsCopying && oldValue != value)
				{
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("IE.Business.JobDeclaration|ZG_RegionOfDestination", Caption = "Region of Destination", ShortCaption = "Dest. Region")]
		public override ZString ZG_RegionOfDestination { get => base.ZG_RegionOfDestination; set => base.ZG_RegionOfDestination = value; }
	}
}
