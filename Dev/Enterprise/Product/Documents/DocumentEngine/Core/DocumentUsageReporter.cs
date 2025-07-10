using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Billing.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine
{
	public class DocumentUsageReporter
	{
		public DocumentUsageReporter()
		{
		}

		public string MenuTitle { get; set; }

		public bool IsPreview { get; set; }

		public bool IsUserSignatureUsed { get; set; }

		public List<(string DeliveryMethod, string AttachmentType)> ContactDetails { get; set; }

		public bool IsTriggeredViaWorkflow => Env.Instance.ServiceTaskCode == "LWK";

		public string CurrentCountry => GlbCompany.CurrentCompany.Country.Code;

		public string TriggeredFrom { get; set; }

		public void ReportForContacts()
		{
			if (ContactDetails != null)
			{
				foreach (var contactDetails in ContactDetails)
				{
					UsageCollector.Report(UsageFeatures.Codes.DocumentGenerated, GetEDIMessageProperties(contactDetails).ToArray());
				}
			}
		}

		List<(string name, object value)> GetEDIMessageProperties((string DeliveryMethod, string AttachmentType) contactDetail)
		{
			var result = new List<(string name, object value)>();
			result.Add((UsageProperties.CountryDocumentCode, CurrentCountry));
			result.Add(("MenuTitle", MenuTitle));
			result.Add(("DeliveryMethod", contactDetail.DeliveryMethod));
			result.Add(("AttachmentType", contactDetail.AttachmentType));
			result.Add(("IsPreview", IsPreview.ToString()));
			result.Add(("IsTriggeredViaWorkflow", IsTriggeredViaWorkflow.ToString()));
			result.Add(("IsUserSignatureUsed", IsUserSignatureUsed.ToString()));
			result.Add(("TriggeredFrom", TriggeredFrom));
			return result;
		}
	}

	public class DocumentUsageDetailsCollector : IService
	{
		public DocumentUsageDetailsCollector()
		{
		}
		public DocumentUsageDetailsCollector(DocumentUsageReporter reporter)
		{
			Reporter = reporter;
		}

		public DocumentUsageReporter Reporter { get; set; }

		public void GetMenuTitle(string menuTitle)
		{
			if (Reporter != null)
			{
				Reporter.MenuTitle = menuTitle;
			}
		}

		public void GetIsPreview(bool isPreview)
		{
			if (Reporter != null)
			{
				Reporter.IsPreview = isPreview;
			}
		}

		public void GetIsUserSignatureUsed(bool isUserSignatureUsed)
		{
			if (Reporter != null)
			{
				Reporter.IsUserSignatureUsed = isUserSignatureUsed;
			}
		}

		public void GetTriggeredFrom(PrintTask printTask)
		{
			if (Reporter != null)
			{
				Reporter.TriggeredFrom = printTask.MostTopLevelBusinessObject?.GetType().Name;
				if (Reporter.TriggeredFrom == null)
				{
					Reporter.TriggeredFrom = (printTask.ParentMenuCommand as DocumentCommand)?.ParentDocumentSupporter?.BusinessObject?.GetType().Name;
				}
			}
		}

		public void GetContactDetails(DocDeliveryContact contact)
		{
			if (Reporter != null && contact != null)
			{
				if (Reporter.ContactDetails == null)
				{
					Reporter.ContactDetails = new List<(string DeliveryMethod, string AttachmentType)>();
				}
				Reporter.ContactDetails.Add((contact.DeliveryMethod, contact.AttachmentType));
			}
		}
	}
}
