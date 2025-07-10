using System.Linq;
using CargoWise.Customs.BR.MessageDefinitions.ImportLicense.Incoming;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseAcceptResponseObjectParent : ImportLicenseResponseObjectParent<loteli>
	{
		public ImportLicenseAcceptResponseObjectParent(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("c760fd82-45c1-4da6-9d69-dff1edd16140", "Load Response from Customs");

		protected override bool ValidateResponse(string fileName, loteli responseData)
		{
			if (responseData == null || responseData.idLote == null || responseData.idLote.Length == 0)
			{
				AddLog(100, 100, Res.GetString("6FE57812-C483-43D9-8515-3CD0EBDF8848", "Unable to read {0}", fileName));
				return false;
			}
			var batchIdentifier = BRMessageHelper.FormatIdLote(responseData.idLote);
			var entries = LoadRelatedEntries(batchIdentifier);
			if (!entries.Any())
			{
				AddLog(100, 100, Res.GetString("a9e6d21c-35ee-4e43-bde9-831a400a6fc2", "The Batch Identifier contained in the XML file ({0}) does not match any Entry of this Job.", responseData.idLote));
				return false;
			}

			var returnList = responseData.listaLIVORetorno;
			var hasValidResponse = returnList?.Any() ?? false;
			if (hasValidResponse)
			{
				var count = 0;
				var total = returnList.Length;
				var importerNumber = Declaration.Importer?.GetCNPJOrCPF() ?? ZString.Empty;

				foreach (var li in returnList)
				{
					count++;
					var entry = entries.Where(x => li.idSolicitacao == x.ImportLicenseIdentifier).FirstOrDefault();

					if (entry == null)
					{
						AddLog(count, total, Res.GetString("1bd33d54-5a85-467d-8a64-4aaed9bfad68", "The Import License Identifier contained in the XML file ({0}) does not match any Entry of this Job.", li.idSolicitacao));
						hasValidResponse = false;
					}
					else if (importerNumber != new ZString(li.importador?.numero).KeepChars(ZString.NumericCharacters))
					{
						AddLog(count, total, Res.GetString("4c84d2aa-a005-4f54-876d-4da801eb0605", "The Importer Registration Number contained in the XML file({0}) does not match the Importer of this Job. The Import License Identifier is {1}.", li.importador?.numero, li.idSolicitacao));
						hasValidResponse = false;
					}
					else if (!entry.MovementReferenceNumber.IsEmpty && entry.MovementReferenceNumber == li.numeroLI)
					{
						AddLog(count, total, Res.GetString("d78fe9d6-8afc-49c7-b08a-e8445a8fc84f", "An XML file containing the following(s) Import License(s) was already loaded. Import License Number: {0}.", li.numeroLI));
						hasValidResponse = false;
					}
					else if (!entry.MovementReferenceNumber.IsEmpty && entry.MovementReferenceNumber != li.numeroLI)
					{
						AddLog(count, total, Res.GetString("86343ec1-6706-44a8-9b59-545dfa9f9bce", "The Entry Header {0} Import License Number ({1}) differs from the Import License Number contained in the XML file ({2}).", entry.CH_BGMReference, entry.MovementReferenceNumber, li.numeroLI));
						hasValidResponse = false;
					}
					else if (entry.CH_Status == BRMessageStatusList.Codes.Rejected && li.numeroLI.Length == 0 && ExistLogRejetedForBatchIdentifier(entry, responseData.idLote))
					{
						AddLog(count, total, Res.GetString("57523bf1-3714-4ae3-82e4-8e237dcbe4d1", "An XML file was already loaded for the Batch Identifier <{0}> and Import License Identifier <{1}>.", responseData.idLote, li.idSolicitacao));
						hasValidResponse = false;
					}
				}

				AddLog(100, 100, Res.GetString("ba467724-5f43-438e-8d9b-009147fa96bb", "File reading complete!"));
			}

			return hasValidResponse;
		}

		protected override void LoadObjects(loteli responseData)
		{
			foreach (var li in responseData.listaLIVORetorno)
			{
				var response = Collection.AddNew();
				response.ReferenceNumber = li.idSolicitacao;
				response.EntryNumber = li.numeroLI;
				response.RegistrationDate = li.dtRegistro;
				response.Diagnosis = li.mensagemDiagnostico?.mensagemDiagnostico?.Length > 0 ? string.Join("; ", li.mensagemDiagnostico.mensagemDiagnostico) : string.Empty;
			}
		}

		protected override ZString InterchangeType => MessageTypeList.Codes.LIC;

		protected override IInboundMessageCreator GetMessageCreator(LoggingInformation logger) => new BRCImportLicenseAcceptInboundMessageCreator(logger);

		protected override BRCResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new BRCImportLicenseAcceptMessageProcessor(logger);

		public override bool ResponseHasDiagnosis => true;

		public override bool ResponseHasReferenceNumber => true;

		ZBool ExistLogRejetedForBatchIdentifier(CusEntryHeader entry, ZString batchIdentifier)
		{
			return entry.Logs.Find(x => x.SL_SE_NKEvent == Events.MessageRejectedCode
						&& x.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, out var number)
						&& number == batchIdentifier).Any();
		}
	}
}
