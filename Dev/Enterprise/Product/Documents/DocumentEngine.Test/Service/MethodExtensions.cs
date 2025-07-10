using System.Collections.Generic;
using System.Text;

namespace Enterprise.DocumentEngine.Service.Testing
{
	static class MethodExtensions
	{
		public static string ToStringForTesting(this IEnumerable<RecipientDetail> recipients)
		{
			var result = new StringBuilder();

			foreach (var recipient in recipients)
			{
				result.AppendLine(string.Format("Name:[{0}]   Organization:[{1}]   DeliveryMethod:[{2}]   AttachmentType:[{3}]   Address:[{4}]   CC:[{5}]   BCC:[{6}]",
					recipient.Name,
					recipient.Organization,
					recipient.DeliveryMethod,
					recipient.AttachmentType,
					recipient.Address,
					recipient.CC,
					recipient.BCC));
			}

			return result.ToString();
		}

		public static string ToStringForTesting(this IEnumerable<PrinterDetail> printers)
		{
			var result = new StringBuilder();

			foreach (var printer in printers)
			{
				result.AppendLine(string.Format("Name:[{0}]   Location:[{1}]   IsPrintAllowed:[{2}]",
					printer.Name,
					printer.Location,
					printer.IsPrintAllowed));
			}

			return result.ToString();
		}

		public static string ToStringForTesting(this IEnumerable<DocumentDetail> documents)
		{
			var result = new StringBuilder();

			foreach (var document in documents)
			{
				result.AppendLine(string.Format("Name:[{0}]   Mode:[{1}]   ShouldInclude:[{2}]",
					document.Name,
					document.Mode,
					document.ShouldInclude));
			}

			return result.ToString();
		}
	}
}
