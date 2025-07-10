using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors
{
	public class EdifactErrorMessageHelper : NonPersistentBusinessObject, ICUSRESV921ESMessageProvider, IExportResponseMessageProvider
	{
		public EdifactErrorMessageHelper(BusinessObjectFactory factory, ZString error, ZString description)
			: base(factory)
		{
			ErrorCode = error;
			ErrorDescription = description;
		}
		ZString ErrorCode { get; }
		ZString ErrorDescription { get; }

		ZString ICUSRESMessageProvider.DocumentMessageName => ZString.Empty;

		ZDateTime ICUSRESMessageProvider.AdmissionDate => ZDateTime.Empty;

		ZString ICUSRESMessageProvider.MessageFunction => ZString.Empty;

		List<ErrorMessage> ICUSRESMessageProvider.FreeTextErrors
		{
			get
			{
				if (freeTextErrorList == null)
				{
					freeTextErrorList = new List<ErrorMessage>()
					{
						new ErrorMessage
						{
							Code = ErrorCode,
							Location = string.Empty,
							Description = ErrorDescription
						}
					};
				}
				return freeTextErrorList;
			}
		}

		List<ErrorMessage> freeTextErrorList;

		ZDateTime ICUSRESV921ESMessageProvider.TransitMaxDate => ZDateTime.Empty;

		ZString ICUSRESV921ESMessageProvider.PrintActionRequired => ZString.Empty;

		ZString ICUSRESV921ESMessageProvider.RegistrationNumber => ZString.Empty;

		ZString ICUSRESV921ESMessageProvider.CSVReleaseCode => ZString.Empty;

		ZDateTime ICUSRESV921ESMessageProvider.CSVReleaseCreationDate => ZDateTime.Empty;

		ZString IExportResponseMessageProvider.UniqueReferenceNumber => ZString.Empty;

		ZString IExportResponseMessageProvider.MessageFunctionCAN => ZString.Empty;

		ZString IExportResponseMessageProvider.CustomsClearanceStatus => ZString.Empty;

		ZString IExportResponseMessageProvider.CSVT2LFCode => ZString.Empty;
	}
}
