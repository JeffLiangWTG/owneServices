using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportJobComInvoiceLineValidation : CommonExportJobComInvoiceLineValidation
	{
		public ExportJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			var (transportDocumentCount, additionalReferenceCount) = GetAdditionalDocumentCounts();
			CheckTransportDocumentCount(transportDocumentCount);
			CheckAdditionalReferenceCount(additionalReferenceCount);
		}

		protected override void CheckJI_Tariff_NoPackage()
		{
			if (Declaration is JobDeclaration dec)
			{
				CheckJI_Tariff_NoPackage_Export(dec);
			}
		}

		protected override void CheckJI_Weight()
		{
			base.CheckJI_Weight();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_WeightInfo);
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			if (!(Parent.JI_LinePrice > 0)
					&& Parent.EntryInstruction is CusEntryInstruction instruction
					&& (instruction.CEI_Style == ExportDeclarationTypeList.Codes.B1 || instruction.CEI_Style == ExportDeclarationTypeList.Codes.B2))
			{
				Parent.JI_LinePriceInfo.AddMessageError(Res.GetString("21DF21D3-A8DC-428A-978D-A39E27E8805B", "You have not entered a Price. Price is needed to execute correct calculation for the required statistical value."));
			}
		}

		(int, int) GetAdditionalDocumentCounts()
		{
			var parent = Parent;
			int transportDocumentCount = 0;
			int additionalReferenceCount = 0;

			if (parent.AdditionalInfos?.Any() == true)
			{
				int index = 0;
				while (index < parent.AdditionalInfos.Count && (transportDocumentCount <= 100 || additionalReferenceCount <= 100) )
				{
					var selectedAdditionalInfo = parent.AdditionalInfos[index];
					if (selectedAdditionalInfo != null)
					{
						if (selectedAdditionalInfo.IsATransportDocument)
						{
							transportDocumentCount++;
						} else if (selectedAdditionalInfo.IsAnAdditionalReference)
						{
							additionalReferenceCount++;
						}
					}
					index++;
				}
			}
			return (transportDocumentCount, additionalReferenceCount);
		}

		void CheckTransportDocumentCount(int transportDocumentCount)
		{
			if (transportDocumentCount > 99)
			{
				Parent.AddRowMessageError(Res.GetString("85E6C90B-9D3B-4065-A080-460AAFF524E7", "The maximum number of transport documents allowed is 99."));
			}
		}

		void CheckAdditionalReferenceCount(int additionalReferenceCount)
		{
			if (additionalReferenceCount > 99)
			{
				Parent.AddRowMessageError(Res.GetString("8EE4E28F-07CF-42AE-94DE-B3B0A8C9A0AA", "The maximum number of additional references allowed is 99."));
			}
		}

		protected override string InvoiceLineNotLinkedToContainerMessage => Res.GetString("C3ECCDD6-CE9F-41E5-B4EF-950F6DE6A14A", "Invoice Line is in containerized mode, but is not linked to a container, a container can be associated to invoice lines from the menu on the Packaging tab -> sub tab Packing Details");
	}
}
