using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class MessageInstruction : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MessageInstruction(BusinessObjectFactory factory, ZBool isWaitingForResponseMessage, ZString validationErrorsMessage, ZString additionalWarningsMessage, BusinessObject businessObject = null, bool showJobReadyForPosting = false, bool isAllowedToSendWithMessageErrors = false)
			: base(factory)
		{
			IsWaitingForResponse = isWaitingForResponseMessage;
			ValidationErrorsMessage = Regex.Replace(validationErrorsMessage, "(?<!\r)\n", "\r\n");
			AdditionalWarningsMessage = Regex.Replace(additionalWarningsMessage, "(?<!\r)\n", "\r\n");
			BusinessObject = businessObject;
			ShowJobReadyForPosting = showJobReadyForPosting;
			IsAllowedToSendWithMessageErrors = isAllowedToSendWithMessageErrors;
		}

		#region Properties

		public ZBool ContainsValidationErrors { get { return !ValidationErrorsMessage.IsEmpty; } }
		public ZBool ContainsAdditionalWarnings { get { return !AdditionalWarningsMessage.IsEmpty; } }

		public ZBool IsWaitingForResponse { get; private set; }
		public ZString ValidationErrorsMessage { get; private set; }
		public ZString AdditionalWarningsMessage { get; private set; }
		public BusinessObject BusinessObject { get; private set; }
		public JobDeclaration BaseJobDeclaration
		{
			get { return this.BusinessObject as JobDeclaration; }
		}
		public ZBool ShowJobReadyForPosting { get; private set; }
		public ZBool IsAllowedToSendWithMessageErrors { get; private set; }

		#endregion
	}
}
