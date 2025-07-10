using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class B3LateSendingWarningProcessor
	{
		public B3LateSendingWarningProcessor(JobDeclaration declaration)
		{
			Argument.NotNull(declaration, "declaration");
			this.declaration = declaration;
			b3SendingNotificationHelper = new B3SendingNotificationHelper(declaration);
		}

		readonly JobDeclaration declaration;
		readonly B3SendingNotificationHelper b3SendingNotificationHelper;

		public void Process()
		{
			var emailBodyText = ZString.Empty;

			if (declaration.IsLVS)
			{
				emailBodyText = Res.GetString("ecbbb01d-24d4-491c-8e8e-f79ac227e703",
					"<html><body><p>This LVS job {0} has not had an Entry or a CAD accepted</p></body></html>",
					b3SendingNotificationHelper.GetHyperLink());
			}
			else if (declaration.JE_EntryAuthorisationDate.IsValid)
			{
				emailBodyText = Res.GetString("961fce13-e2dd-4ba9-b7f3-9150483026f4",
					"<html><body><p>This job {0} has not had a CAD accepted and it is now {1} working days since release</p></body></html>",
					b3SendingNotificationHelper.GetHyperLink(), declaration.CA_AccountingAge);
			}

			if (!emailBodyText.IsEmpty)
			{
				b3SendingNotificationHelper.SendEmail(Res.GetString("f4a967a8-1f4f-4607-a178-2380a7220685", "CAD Late Sending Warning"), emailBodyText);
				declaration.Logs.AddNew(AutoEvents.CanadianCADLateWarningSent, "CADWarning Reported");
			}
		}
	}
}
