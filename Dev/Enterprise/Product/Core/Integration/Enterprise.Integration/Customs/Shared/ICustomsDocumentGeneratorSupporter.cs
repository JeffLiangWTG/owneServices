using System;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICustomsDocumentGeneratorSupporter
		{
			Guid BranchPK { get; }
			ZString JobReference { get; }

			ZString GetDocumentName(ZString actionCode);
			ZBool GenerateCustomsDocument(ZString actionCode);
			ZString GetReasonForUnableToGenerateCustomsDocument();
		}
	}
}
