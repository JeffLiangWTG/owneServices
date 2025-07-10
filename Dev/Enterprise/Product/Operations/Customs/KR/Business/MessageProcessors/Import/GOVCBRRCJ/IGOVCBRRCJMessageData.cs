using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRRCJMessageData
	{
		ZDateTime NoticeDateTime { get; }
		ZString SuspendedNumber { get; }
		ZString ImportDeclarationNumber { get; }
		ZString OriginalDeclarationNumber { get; }
		ZString SuspendedType { get; }
		ZString SuspendedReason { get; }
		ZDate DeclarationDate { get; }
		ZDate SuspendedDate { get; }
		ZString SuspendedCode { get; }
		ZString SuspendedSolutionCode { get; }
		ZString DeclarantCompanyName { get; }
		ZString ImportCompanyName { get; }
		ZString ImportRepresentativeName { get; }
		ZString DeclarationOffice { get; }
		ZString CustomsManagerName { get; }
		ZString CustomsManagerPhoneNumber { get; }
		ZString CustomsPersonFaxNumber { get; }
		IEnumerable<IConsignment> Consignment { get; }
	}

	interface IConsignment
	{
		ZInt EntryLineNo { get; }
		ZString HSCode { get; }
		ZString InvoiceDescription { get; }
		ZString BrandName { get; }
	}

	class GOVCBRRCJMessageData : IGOVCBRRCJMessageData
	{
		public ZDateTime NoticeDateTime { get; set; }
		public ZString SuspendedNumber { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZString OriginalDeclarationNumber { get; set; }
		public ZString SuspendedType { get; set; }
		public ZString SuspendedReason { get; set; }
		public ZDate DeclarationDate { get; set; }
		public ZDate SuspendedDate { get; set; }
		public ZString SuspendedCode { get; set; }
		public ZString SuspendedSolutionCode { get; set; }
		public ZString DeclarantCompanyName { get; set; }
		public ZString ImportCompanyName { get; set; }
		public ZString ImportRepresentativeName { get; set; }
		public ZString DeclarationOffice { get; set; }
		public ZString CustomsManagerName { get; set; }
		public ZString CustomsManagerPhoneNumber { get; set; }
		public ZString CustomsPersonFaxNumber { get; set; }
		public IEnumerable<IConsignment> Consignment { get; set; }
	}

	class Consignment : IConsignment
	{
		public ZInt EntryLineNo { get; set; }
		public ZString HSCode { get; set; }
		public ZString InvoiceDescription { get; set; }
		public ZString BrandName { get; set; }
	}
}
