using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Messaging.MessageBuilders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class EDIReleaseImportMessageManager : CAMessageManager
	{
		public EDIReleaseImportMessageManager(IEDIReleaseOGD importDeclarationEDIReleaseWrapper, IUserNotification notification)
			: base(importDeclarationEDIReleaseWrapper, new EDIReleaseImportStatusCalculator(), notification)
		{
		}

		protected override ValidateForMessageType GetValidationType()
		{
			return ValidateForMessageType.ACROSS;
		}

		#region Overrides of SingleMessageManager

		#region MessageFriendlyName

		public override string MessageFriendlyName
		{
			get
			{
				string messageDescription;
				string assessmentOptionDescription;
				var releaseValidator = new EDIReleaseValidator(DataWrapper.ServiceOptionID, DataWrapper.AssessmentOption);
				if (releaseValidator.AllOptionsSpecifiedAndValid() && releaseValidator.IsCombinationValid())
				{
					messageDescription = new ServiceOptions().GetDescriptionFromCode(DataWrapper.ServiceOptionID);
					assessmentOptionDescription = GetAssessmentOptionDescription();
				}
				else
				{
					messageDescription = Res.GetString("bc843f3d-385a-48f2-9ba2-7cbeab40a140", "ACROSS");
					assessmentOptionDescription = string.Empty;
				}
				return Res.GetString("331c6193-1414-4ad6-9bf5-1e712f3e2b9b", "{0}{1} for {2}", messageDescription, assessmentOptionDescription, DataWrapper.TransactionNumber);
			}
		}

		string GetAssessmentOptionDescription()
		{
			string description;
			switch (DataWrapper.AssessmentOption)
			{
				case AssessmentOptions.Codes.AQtoFollow:
					description = " " + Res.GetString("b5037eb5-a455-4898-894c-d69b5ff568a6", "(AQ to follow)");
					break;
				case AssessmentOptions.Codes.AppraisalQualityData:
					description = " " + Res.GetString("e56407fc-7e86-469b-8978-ee1ec2ba0dec", "(Appraisal Quality)");
					break;
				default:
					description = string.Empty;
					break;
			}
			return description;
		}

		#endregion

		#endregion

		#region Overrides of CAMessageManager

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return new EDIReleaseMessageBuilder(actionCode, DataWrapper);
		}

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			var result = base.CanSendThisMessage(actionCode, out messageText);
			if (result)
			{
				var declaration = BusinessObject as JobDeclaration;
				result = IsCreditCheckOKToSend(declaration, out messageText);
			}
			if (result)
			{
				var releaseValidator = new EDIReleaseValidator(DataWrapper.ServiceOptionID, DataWrapper.AssessmentOption);
				if (!(result = releaseValidator.AllOptionsSpecifiedAndValid() && releaseValidator.IsCombinationValid()))
				{
					messageText = releaseValidator.LastErrorMessage;
				}
			}
			if (result)
			{
				if (!(result = DataWrapper.Invoices.Sum(x => x.InvoiceLines.Count()) < 1000))
				{
					messageText = Res.GetString("52233AC0-5BBD-4CFC-95B7-7A8A26F732DC", "ACROSS entry does not allow more than 999 lines.");
				}
			}
			return result;
		}

		protected override ZString GetAdditionalWarningsMessage(MessageSubTypes actionCode)
		{
			var builder = new ZStringBuilder();
			var declaration = BusinessObject as JobDeclaration;
			if (declaration != null)
			{
				var timeAtPort = declaration.TimeAtPortOfDischarge;
				if (actionCode == MessageSubTypes.Change && !declaration.JE_DateOfArrival.IsEmpty &&
					timeAtPort > declaration.JE_DateOfArrival
					|| declaration.JE_EntryStatus == EDIReleaseImportEntryStatusList.Codes.GoodsReleased
					|| declaration.JE_EntryStatus == EDIReleaseImportEntryStatusList.Codes.Y51ReleaseDocumentsRequired)
				{
					builder.Append(Res.GetString("3d565c2c-6c33-4da7-97cd-117c70864418",
						"This job is already clear or has had an ACROSS entry lodged and has already arrived. Are you sure you wish to amend this release entry?"));
				}

				var dateOfFirstArrival = declaration.JE_DateOfFirstArrival;
				var dateOfArrival = declaration.JE_DateOfArrival;
				if (dateOfFirstArrival.IsValid)
				{
					if (!timeAtPort.IsValid)
					{
						builder.Append(Res.GetString("e21835a8-45ef-494e-9383-6484942d0d25",
								"Please check the Time Zone of Arrival Port or your Home Port, it should not be empty."));
					}
					else
					{
						if (dateOfFirstArrival > timeAtPort.AddDays(30))
						{
							builder.Append(Res.GetString("89662934-B051-4007-A297-82AED7E93B96",
								"ETA First Port of Arrival date cannot be farther than 30 days in the future."));
						}
						else if (dateOfFirstArrival > timeAtPort.AddHours(72) || dateOfFirstArrival < timeAtPort.AddHours(4))
						{
							if (declaration.InvoiceLines.OfType<JobComInvoiceLine>().Any(
								line => line.JI_Tariff.StartsWith("02", StringComparison.Ordinal) && line.InvoiceHeader.CA_RN_NKExport == Core.Constants.CountryCodes.UnitedStates))
							{
								builder.Append(Res.GetString("2213E5C6-44A9-40EB-AD80-08929AA1B9ED",
									"This product from the US is outside the allowable time frame for reporting (more than 72 hours or less than 4 hours)."));
							}
						}
					}
				}
				else if (dateOfArrival.IsValid && timeAtPort.Date < dateOfArrival.Date)
				{
					builder.Append(Res.GetString("9CCE50F9-0443-434A-8887-DC07B4DA9CFA", "This shipment has not yet arrived."));
				}
			}

			if (actionCode == MessageSubTypes.Withdraw)
			{
				builder.Append(Res.GetString("9860DB27-B7D3-4E7C-A8AD-2C02D84D40EF", "Electronic Cancel is only allowed for PARS entries that have not arrived, or for post arrival RMDs that are in a rejected state. You should contact your local CBSA office if you wish to cancel a PARS that has arrived, or a post arrival RMD that has been accepted."));
			}

			return builder.ToStringWithDelimiterBetweenAppends("\r\n");
		}

		new IEDIReleaseOGD DataWrapper
		{
			get { return (IEDIReleaseOGD)base.DataWrapper; }
		}

		protected override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CAACROSSMsgSend; }
		}

		protected override void OnMessageQueuedForSending(MessageSubTypes actionCode)
		{
			base.OnMessageQueuedForSending(actionCode);
			if (actionCode == MessageSubTypes.Create)
			{
				var messageWrapper = DataWrapper as EDIReleaseImportMessageWrapper;
				if (messageWrapper != null)
				{
					messageWrapper.PopulateEntrySubmittedDateIfRequired();
				}
			}
		}

		#endregion
	}
}
