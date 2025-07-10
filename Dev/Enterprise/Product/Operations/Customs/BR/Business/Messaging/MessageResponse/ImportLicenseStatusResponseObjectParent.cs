using System.Linq;
using CargoWise.Customs.BR.MessageDefinitions.ImportLicense.Incoming;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseStatusResponseObjectParent : ImportLicenseResponseObjectParent<respostaconsultali>
	{
		public ImportLicenseStatusResponseObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("B2A169A7-B8FA-496E-A8A5-137056D0CEDA", "Update Import License Status");

		protected override ZString InterchangeType => MessageTypeList.Codes.LIS;

		protected override IInboundMessageCreator GetMessageCreator(LoggingInformation logger) => new BRCImportLicenseStatusInboundMessageCreator(logger);

		protected override BRCResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new BRCImportLicenseStatusMessageProcessor(logger);

		public override bool ResponseHasStatus => true;

		protected override bool ValidateResponse(string fileName, respostaconsultali responseData)
		{
			if (responseData == null || responseData.identificadorconsulta == null || !(responseData.Item is listalicompletatype))
			{
				AddLog(100, 100, Res.GetString("159B097F-B1BA-460B-86B4-0A7A2DC0F729", "Unable to read {0}", fileName));
				return false;
			}

			var response = responseData.Item as listalicompletatype;

			var hasValidEntries = response?.licompleta.Any() ?? false;

			if (hasValidEntries)
			{
				var totalCount = response.licompleta.Length;
				var loadedCount = 0;
				foreach (var licompleta in response.licompleta)
				{
					loadedCount++;
					var entryNumber = new ZString(licompleta.GrupoDadosBasicos?.numeroli).KeepNumericCharacters();
					var entry = Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(x => x.MovementReferenceNumber == entryNumber);

					if (entry == null)
					{
						AddLog(loadedCount, totalCount, Res.GetString("2BAA7D59-2A61-455D-83B2-2AA9D8CC0AD9", "The Import License Number {0} does not match any Entry Header.", entryNumber));
						hasValidEntries = false;
					}
				}

				AddLog(100, 100, Res.GetString("EADEAAFA-72E5-4736-B649-6127EF3F3FAB", "File loading complete!"));
			}

			return hasValidEntries;
		}

		protected override void LoadObjects(respostaconsultali responseData)
		{
			var response = responseData.Item as listalicompletatype;
			var hasValidEntries = response?.licompleta.Any() ?? false;

			if (hasValidEntries)
			{
				foreach (var li in response.licompleta)
				{
					var responseObject = Collection.AddNew();
					responseObject.EntryNumber = li.GrupoDadosBasicos?.numeroli;
					responseObject.RegistrationDate = li.GrupoLIAnuencias?.InformacoesLI?.dataregistro + " " + li.GrupoLIAnuencias?.InformacoesLI?.horaregistro;
					responseObject.Status = li.GrupoLIAnuencias?.InformacoesLI?.nomesituacao;
				}
			}
		}
	}
}
