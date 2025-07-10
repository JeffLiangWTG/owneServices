using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	[DebuggerDisplay("EnglishMessage = {EnglishMessage}, LocalLanguageMessage = {LocalLanguageMessage}")]
	public class DocARInvoiceTaxMessage : DocBaseWrapper
	{
		protected DocARInvoiceTaxMessage(AccInvMsg aRInvoiceMessage, BusinessObjectFactory factoryToWrap)
			: base(aRInvoiceMessage, factoryToWrap)
		{
			this.AccInvoiceMessage = aRInvoiceMessage;
		}

		protected DocARInvoiceTaxMessage(ZString englishMessage, ZString localLanguageMessage, BusinessObjectFactory factoryToWrap)
			: base(null, factoryToWrap)
		{
			fEnglishMessage = englishMessage;
			fLocalLanguageMessage = localLanguageMessage;
		}

		public static class Schema
		{
			public const string EnglishMessage = "EnglishMessage";
			public const string LocalLanguageMessage = "LocalLanguageMessage";
		}

		internal readonly AccInvMsg AccInvoiceMessage;
		readonly ZString fEnglishMessage;
		readonly ZString fLocalLanguageMessage;

		public static DocARInvoiceTaxMessage New(AccInvMsg invoiceMessage, BusinessObjectFactory factoryToWrap)
		{
			DocARInvoiceTaxMessage result = null;
			if (invoiceMessage != null)
			{
				result = new DocARInvoiceTaxMessage(invoiceMessage, factoryToWrap);
			}
			return result;
		}

		public static DocARInvoiceTaxMessage New(ZString englishMessage, ZString localLanguageMessage, BusinessObjectFactory factoryToWrap)
		{
			return new DocARInvoiceTaxMessage(englishMessage, localLanguageMessage, factoryToWrap);
		}

		public ZString EnglishMessage
		{
			get { return AccInvoiceMessage == null ? fEnglishMessage : AccInvoiceMessage.A9_EnglishMsgMultilingual; }
		}

		public ZString LocalLanguageMessage
		{
			get { return AccInvoiceMessage == null ? fLocalLanguageMessage : AccInvoiceMessage.A9_LocalMsg; }
		}
	}
}
