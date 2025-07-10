using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G3;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public abstract class G3CommonMessagePrettyFormatter : CommonMessagePrettyFormatter
	{
		protected void AppendListOfErrors(IG3CommonErrors response, StringBuilder messageDetails)
		{
			if (response.Errors.Count > 0)
			{
				messageDetails.Append(RejectedDeclarationText);
				messageDetails.Append(ListOfErrorsText);

				var tableCreator = GetNewTableCreator();
				tableCreator.WriteRow(ErrorCodeColumnText, ErrorPointerColumnText, DescriptionColumnText);

				foreach (var error in response.Errors)
				{
					tableCreator.WriteRow(error.Code, error.Pointer, error.Description);
				}

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected string RejectedText => GetH3Text(ResString.GetMultilingualString("E6AF99F4-B34D-4C80-9513-2AA1AD413492", "Rejected"));
		protected string LrnText => ResString.GetMultilingualString("AAABA9F3-E603-4644-B0AC-7F8247936DAA", "LRN:");
	}
}
