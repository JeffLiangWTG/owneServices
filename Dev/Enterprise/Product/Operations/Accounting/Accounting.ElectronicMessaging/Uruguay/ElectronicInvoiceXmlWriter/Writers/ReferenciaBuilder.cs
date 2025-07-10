using System;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.CountryCompliance.UruguayComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	public interface IReferenciaBuilder
	{
		ReferenciaReferencia[] BuildReferenciaInfo(TransactionInfo transaction);
	}

	class ReferenciaBuilder : IReferenciaBuilder
	{
		public ReferenciaBuilder()
		{
			CFEHelper_constructorInitializedOnly = new CFEHelper();
		}

		ICFEHelper CFEHelper => CFEHelper_constructorInitializedOnly;
		ICFEHelper CFEHelper_constructorInitializedOnly;

#if DEBUG
		public void SubstituteCFEHelper_ForTestOnly(ICFEHelper replacement) => CFEHelper_constructorInitializedOnly = replacement;
		public ICFEHelper CFEHelper_ExposedForTestOnly => CFEHelper;
#endif

		ReferenciaReferencia[] IReferenciaBuilder.BuildReferenciaInfo(TransactionInfo transaction)
		{
			if (IsCreditOrDebitNote(transaction?.ComplianceSubType))
			{
				var referencia = new ReferenciaReferencia()
				{
					NroLinRef = "1",
				};

				if (transaction.OriginalReference != null)
				{
					var tpoDocRef = GetTipoDocRef(transaction.OriginalReference);

					if (tpoDocRef.HasValue)
					{
						referencia.TpoDocRef = tpoDocRef.Value;
						referencia.TpoDocRefSpecified = true;

						if (transaction.OriginalReference.OriginalTransactionReference.HasValue)
						{
							var (serie, numero) = CFEHelper.GetInvoiceSerieAndNumber(transaction.OriginalReference.OriginalTransactionReference.Value);
							referencia.Serie = serie;
							referencia.NroCFERef = numero;
						}
						else if (transaction.OriginalReference.OriginalTransactionNumber.HasValue && !transaction.OriginalReference.OriginalTransactionComplianceSubType.HasValue)
						{
							var (serie, numero) = CFEHelper.GetInvoiceSerieAndNumber(transaction.OriginalReference.OriginalTransactionNumber.Value.Substring(3));
							referencia.Serie = serie;
							referencia.NroCFERef = numero;
						}
					}
				}

				return new ReferenciaReferencia[] { referencia };
			}
			else
			{
				return null;
			}
		}

		bool IsCreditOrDebitNote(ZString? complianceSubTypee)
		{
			switch (complianceSubTypee)
			{
				case ComplianceSubTypeCodes.TCR:
				case ComplianceSubTypeCodes.TCD:
				case ComplianceSubTypeCodes.YCR:
				case ComplianceSubTypeCodes.YCD:
				case ComplianceSubTypeCodes.YKR:
				case ComplianceSubTypeCodes.YKD:
				case ComplianceSubTypeCodes.TKD:
				case ComplianceSubTypeCodes.TKC:
					return true;
				default:
					return false;
			}
		}

		CFEType? GetTipoDocRef(OriginalReference originalReference)
		{
			CFEType? cfEType = null;

			if (originalReference.OriginalTransactionComplianceSubType.HasValue)
			{
				switch (originalReference.OriginalTransactionComplianceSubType)
				{
					case ComplianceSubTypeCodes.TXI:
						cfEType = CFEType.Item111;
						break;
					case ComplianceSubTypeCodes.TCR:
						cfEType = CFEType.Item112;
						break;
					case ComplianceSubTypeCodes.TCD:
						cfEType = CFEType.Item113;
						break;
					case ComplianceSubTypeCodes.TKT:
						cfEType = CFEType.Item101;
						break;
					case ComplianceSubTypeCodes.TKC:
						cfEType = CFEType.Item102;
						break;
					case ComplianceSubTypeCodes.TKD:
						cfEType = CFEType.Item103;
						break;
					case ComplianceSubTypeCodes.YXI:
						cfEType = CFEType.Item211;
						break;
					case ComplianceSubTypeCodes.YCR:
						cfEType = CFEType.Item212;
						break;
					case ComplianceSubTypeCodes.YCD:
						cfEType = CFEType.Item213;
						break;
					case ComplianceSubTypeCodes.YKT:
						cfEType = CFEType.Item201;
						break;
					case ComplianceSubTypeCodes.YKR:
						cfEType = CFEType.Item202;
						break;
					case ComplianceSubTypeCodes.YKD:
						cfEType = CFEType.Item203;
						break;
				}
			}
			else if (originalReference.OriginalTransactionNumber.HasValue)
			{
				var number = originalReference.OriginalTransactionNumber.Value.Substring(0, 3);
				if (Enum.TryParse((NoResString)"Item" + number, out CFEType cFETypeEnumValue)) // Enum Constant Data Format
				{
					cfEType = cFETypeEnumValue;
				}
			}

			return cfEType;
		}
	}
}
