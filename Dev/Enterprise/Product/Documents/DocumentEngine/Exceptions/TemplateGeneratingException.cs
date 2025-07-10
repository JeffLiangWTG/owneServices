using System;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class TemplateGeneratingException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		internal TemplateGeneratingException(string message, FlexCelXlsAdapterException innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected TemplateGeneratingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public string GetErrorMessage(DocumentCommand documentCommand)
		{
			if (documentCommand != null)
			{
				return Res.GetString("2AD79E73-2119-4DE6-8377-8A62C7D6CBFE",
					"Error generating the template when delivering the following document menu item:\r\n\tMenu Path: {0}\r\n\tMenu Name: {1}\r\n\tSystem Defined: {2}\r\n\tParent: {3}\r\n\r\nThe following error occurred:\r\n{4}\r\nPlease check your {5} template.",
					documentCommand.SU_MenuPath, documentCommand.SU_MenuName, documentCommand.SU_IsSystemDefined.ToYN(), documentCommand.Parent?.DocumentSupporter?.BusinessObject?.HumanReadableName, InnerException?.Message, Core.Constants.SectionRepositoryTemplateNames.User);
			}

			return InnerException?.Message;
		}

		#region Constructor For IJsonSerializable

		internal TemplateGeneratingException(TemplateGeneratingExceptionJsonData data)
			: base(data.Message, new Exception(data.InnerExceptionMessage))
		{
		}

		#endregion

		public object GetJsonData() => new TemplateGeneratingExceptionJsonData()
		{
			Message = base.Message,
			InnerExceptionMessage = base.InnerException?.Message,
		};
	}
}
