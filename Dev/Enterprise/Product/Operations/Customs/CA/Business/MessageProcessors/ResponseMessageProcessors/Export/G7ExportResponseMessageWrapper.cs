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
	class G7ExportResponseMessageWrapper
	{
		internal G7ExportResponseMessageWrapper(EXPEDIMessage message)
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

		internal ZString CERSProofOfReportNumber
		{
			get { return D00AMessageUtilities.GetReference(cusres.Group3, ReferenceFunctionCodeQualifierList.ExportDeclaration); }
		}

		internal IEnumerable<ErrorMessage> ErrorMessages
		{
			get
			{
				return from pair in D00AMessageUtilities.GetErrorCodesAndRejectComments(cusres.Group4)
					   select new ErrorMessage(pair[0], message.GetErrorDescription(pair[0]), pair[1]);
			}
		}

		internal bool IsSyntaxError
		{
			get { return string.IsNullOrEmpty(D00AMessageUtilities.GetDocumentName(cusres.BGM)) || D00AMessageUtilities.CheckIsSyntaxError(cusres.Group4); }
		}

		#region ErrorMessage

		internal class ErrorMessage : ITableInterpretation
		{
			internal ErrorMessage(string code, string description, string comments)
			{
				this.code = code;
				this.description = description;
				this.comments = comments;
			}

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return Res.GetString("81d522ed-ffd3-477c-92a5-250e8a8b4db1", "Error Messages"); }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get
				{
					yield return Res.GetString("54980b00-9a0e-4e7f-ace5-ec0cd0cde091", "Code");
					yield return Res.GetString("ae9286a4-e936-43a7-827b-39d663291d4d", "Error");
					yield return Res.GetString("267fba89-74ce-4f47-9cf0-a97327cc3783", "Error Data Value");
				}
			}

			IEnumerable<object> ITableValues.Values
			{
				get { return new[] { code, description, comments }; }
			}

			#endregion

			readonly string code;
			readonly string description;
			readonly string comments;
		}

		#endregion

		readonly EXPEDIMessage message;
		readonly CUSRESMessage cusres;
	}
}

//Tested in G7ExportResponseMessageProcessor
