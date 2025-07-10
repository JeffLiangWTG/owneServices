using System.Text;
using CargoWise.Types;

using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.DataImport
{
	public class InvoiceDetailsLine : UPEDataLine
	{
		public InvoiceDetailsLine(UPEManifestImporter importer, ExportCustomsManifestHeader header, string line)
			: base(importer, header, line)
		{
		}

		internal bool IsMainInvoice()
		{
			return (line.Substring(44, 6) == InvoiceConstants.MainInvoiceType);
		}

		public override ZBool ValidateLine()
		{
			return line.Length > 160;
		}

		#region Invoice Constants

		public static class InvoiceConstants
		{
			public const string MainInvoiceType = "500000";
			public const string CANPrefix = "ECN#";
		}

		#endregion

		#region Implementation

		protected override void DoProcessing()
		{
			// Main Invoice (Invoice line 1 should contain ECN#)
			if (IsMainInvoice())
			{
				CAN = "";
				ZString tempCAN = line.Substring(109, 52).Trim(' ');
				if (tempCAN.StartsWith(InvoiceConstants.CANPrefix))
				{
					CAN = tempCAN.SubstringSafe(InvoiceConstants.CANPrefix.Length, tempCAN.Length - InvoiceConstants.CANPrefix.Length).Trim(' ');
					CAN = CAN.Left(ExportCustomsManifestLines.Schema.EL_CANMaxLength);
					if (CAN.StartsWith("EX"))
					{
						CAN = CMRExportExemptionCodes.GetFromExit2Exemption(CAN);
					}
				}

				if (!string.IsNullOrEmpty(PackageTrackingNumber) && string.IsNullOrEmpty(CAN))
				{
					CAN = GetCANFromTranshipmentNumber();
				}

				if (CAN == "")
				{
					SetForEmptyCAN();
					SetDescription(SafeSubstring(line, 57, 52).Trim(' '));
				}
				else
				{
					// If CAN line is duplicated, ignore the new one
					if (header.CurrentManifestLine == null || header.CurrentManifestLine.EL_CAN != CAN)
					{
						SetCAN(CAN);
						SetDescription(SafeSubstring(line, 57, 52).Trim(' '));
					}
				}
			}
			else
			{
				SetDescription(SafeSubstring(line, 57, 104).Trim(' '));
			}

			SetDestinationCountry(SafeSubstring(line, 6, 2));
		}

		internal protected void SetDescription(ZString description)
		{
			if (header.CurrentManifestLine != null)
			{
				// Concatenate up to 128 characters
				int noOfSpaceLeft = 126 - header.CurrentManifestLine.EL_GoodsDescription.Length;
				if (noOfSpaceLeft > 0)
				{
					StringBuilder descBuilder = new StringBuilder(header.CurrentManifestLine.EL_GoodsDescription);

					if (!header.CurrentManifestLine.EL_GoodsDescription.IsEmpty)
					{
						descBuilder.Append("; ");
					}

					if (description.Length > noOfSpaceLeft)
					{
						descBuilder.Append(description.Substring(0, noOfSpaceLeft));
					}
					else
					{
						descBuilder.Append(description);
					}

					header.CurrentManifestLine.EL_GoodsDescription = descBuilder.ToString();
				}
			}
		}

		internal protected void SetDestinationCountry(ZString countryCode)
		{
			if (header.CurrentManifestLine != null)
			{
				if (header.CurrentManifestLine.EL_RN_NKCountryOfDestination.IsEmpty)
				{
					header.CurrentManifestLine.EL_RN_NKCountryOfDestination = countryCode;
				}
			}
		}

		protected ZString CAN;

		internal ZString PackageTrackingNumber { get; set; }

		internal ZString GetCANFromTranshipmentNumber()
		{
			ZString result = string.Empty;
			var query = new CargoWise.EntityFramework.ZQuery();
			query.AddToFilter(CusHAWBSchema.CS_HAWB, PackageTrackingNumber);
			query.OrderBy = CusHAWBSchema.CS_SystemCreateTimeUtc.Name + CargoWise.EntityFramework.OrderByClause.Descending;
			var hawb = header.Factory.LoadTop1<Customs.Business.CusHAWB>(query);

			if (hawb != null && !string.IsNullOrEmpty(hawb.CS_TranshipmentEntryNum))
			{
				result = hawb.CS_TranshipmentEntryNum.Left(ExportCustomsManifestLines.Schema.EL_CANMaxLength);
			}

			return result;
		}

		#endregion
		#region Implementation
		#endregion

	}
}
