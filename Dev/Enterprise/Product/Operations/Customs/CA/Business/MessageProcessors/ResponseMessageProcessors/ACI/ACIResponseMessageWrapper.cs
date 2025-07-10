using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D00A.Messages.CUSRES;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	class ACIResponseMessageWrapper
	{
		internal ACIResponseMessageWrapper(ACIEDIMessage message)
		{
			this.message = message;
			Argument.NotNull(message, "message");
			cusres = message.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet()) as CUSRESMessage;
			Argument.NotNull(cusres, "message", "Supported EDIFACT message type is D00A CUSRES");
		}

		internal ZString DocumentReference
		{
			get { return D00AMessageUtilities.GetDocumentReference(cusres.BGM); }
		}

		internal ZDateTime ProcessingDate
		{
			get { return D00AMessageUtilities.GetDate(cusres.DTM, DateOrTimeOrPeriodFunctionCodeQualifierList.ProcessingDateTime); }
		}

		internal ZString ProcessingIndicator
		{
			get { return D00AMessageUtilities.GetProcessingIndicator(cusres.GIS); }
		}

		internal ZString RelatedCargoControlNumber
		{
			get { return D00AMessageUtilities.GetReference(cusres.Group3, ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber); }
		}

		internal ZString RiskAssessmentType
		{
			get
			{
				var typeCode = D00AMessageUtilities.GetErrrorIdentifier(cusres.Group4);
				return ProcessingIndicator != "25" || string.IsNullOrEmpty(typeCode) ? string.Empty
						: (new RiskAssessmentTypes().GetDescriptionFromCode(typeCode) ?? RiskAssessmentTypes.Descriptions.Unknown);
			}
		}

		internal ZString ContainerNumbers
		{
			get
			{
				const int containersPerLine = 6;
				var builder = new ZStringBuilder();
				var containers = D00AMessageUtilities.GetEquipment(cusres.Group6, EquipmentTypeCodeQualifierList.Container);

				ZStringBuilder lineBuilder;
				while (!(lineBuilder = new ZStringBuilder(containers.Take(containersPerLine))).IsEmpty)
				{
					builder.Append(lineBuilder.ToStringWithDelimiterBetweenAppends(", "));
					containers = containers.Skip(containersPerLine);
				}
				return builder.ToStringWithDelimiterBetweenAppends(",\r\n");
			}
		}

		internal IEnumerable<Notification> Notifications
		{
			get { return from pair in D00AMessageUtilities.GetErrorCodesAndRejectComments(cusres.Group4) select new Notification(this, pair[0], pair[1]); }
		}

		internal bool IsSyntaxError
		{
			get { return D00AMessageUtilities.CheckIsSyntaxError(cusres.Group4); }
		}

		#region Notification

		internal class Notification : ITableInterpretation
		{
			internal Notification(ACIResponseMessageWrapper wrapper, string code, string comments)
			{
				this.wrapper = wrapper;
				this.code = code;
				this.comments = comments;
			}

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get
				{
					switch (Type)
					{
						case RiskAssessment:
							return Res.GetString("e43e31d2-ecca-453e-a130-8ccd41c751b2", "Risk Assessment Notices");
						case NoMatch:
							return Res.GetString("6f826427-1b1f-4484-80f5-033439da35df", "No-Match Notices");
						default:
							return Res.GetString("2129c622-aca8-45cc-8861-2e8cd760f76d", "Error Messages");
					}
				}
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get
				{
					yield return Res.GetString("4d5be95b-450e-4103-91d7-774f2f032539", "Code");
					switch (Type)
					{
						case RiskAssessment:
							yield return Res.GetString("4cc0059f-d77d-4ccb-90e7-0c80943f9db5", "Notice Details");
							yield return Res.GetString("d2b78fae-89b3-4f9b-8217-cf789075f521", "Officer Remarks");
							break;
						case NoMatch:
							yield return Res.GetString("8c69982b-e86e-44fd-87cc-b81b7291ad4c", "No-Match Reason");
							yield return Res.GetString("84450906-1c16-4738-b3e6-f82be59d8c59", "Comments");
							break;
						default:
							yield return Res.GetString("74ce420f-a7fd-48a4-9fef-9fbea29a05c3", "Error");
							yield return Res.GetString("33b92c4b-afbe-426e-9049-f99e619a1476", "Error Data Value");
							break;
					}
				}
			}

			IEnumerable<object> ITableValues.Values
			{
				get { return new[] { code, wrapper.message.GetErrorDescription(Type + code), comments }; }
			}

			#endregion

			#region Implementation

			string Type
			{
				get
				{
					switch (wrapper.ProcessingIndicator)
					{
						case "25":
							return RiskAssessment;
						case "33":
							return NoMatch;
						default:
							return Error;
					}
				}
			}

			internal const string Error = "";
			internal const string RiskAssessment = "RA";
			internal const string NoMatch = "UL";

			#endregion

			readonly string code;
			readonly string comments;
			readonly ACIResponseMessageWrapper wrapper;
		}

		#endregion

		readonly ACIEDIMessage message;
		readonly CUSRESMessage cusres;
	}
}

//Tested in SupplementaryCargoReportResponseMessageProcessor
