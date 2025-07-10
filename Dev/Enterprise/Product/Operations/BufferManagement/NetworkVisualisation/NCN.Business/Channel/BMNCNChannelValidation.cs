using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNChannelValidation : AutoBMNCNChannelValidation
	{
		public BMNCNChannelValidation(AutoBMNCNChannel parent)
			: base(parent)
		{
		}

		new BMNCNChannel Parent => (BMNCNChannel)base.Parent;

		protected override void CheckBNL_Name()
		{
			base.CheckBNL_Name();

			MandatoryValidation.CheckEntered(Parent.BNL_NameInfo);

			var allChannels = Parent.Diagram?.Channels;

			if (allChannels != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.BNL_NameInfo, allChannels, Res.GetString("bd04fc4b-99cd-4ca7-b2a1-7a95d0f75af2", "Channel names must be unique for each diagram. Please enter a unique name."));
			}
		}

		protected override void CheckBNL_Sequence()
		{
			base.CheckBNL_Sequence();

			CompareValidation.CheckNumberGreaterThanZero(Parent.BNL_SequenceInfo);

			var allChannels = Parent.Diagram?.Channels;

			if (allChannels != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.BNL_SequenceInfo, allChannels, Res.GetString("52b72019-0342-4b6c-bac8-e7fa5cfc3035", "Each channel must have a unique sequence greater than 0. Please enter a unique sequence."));
			}
		}

		protected override void CheckBNL_Height()
		{
			base.CheckBNL_Height();

			CompareValidation.CheckGreaterThanOrEqualTo(Parent.BNL_HeightInfo, CCPMConstants.MinimumChannelHeight);
			CompareValidation.CheckLessThanOrEqualTo(Parent.BNL_HeightInfo, CCPMConstants.MaximumChannelHeight);

			var shapes = Parent.Shapes;

			if (shapes.Any())
			{
				var tallShapes = shapes.Where(s => s.Height > Parent.BNL_Height);
				var tallestShapeHeight = tallShapes.Any() ? tallShapes.Max(s => s.Height) : 0;

				if (tallestShapeHeight > 0)
				{
					Parent.BNL_HeightInfo.AddError(Res.GetString("5fbd5285-fa4a-48ec-8ef9-2b0837d2200f", "The specified height is too small to contain all of the shapes within this Channel, since the tallest shape has a height of {0}.", tallestShapeHeight));
				}
			}
		}
	}
}
