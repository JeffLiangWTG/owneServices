using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.Business
{
	public static class EDocHelper
	{
		public static void ChangeExistingEDocsFileNames(IDocManagerSupport businessObject, Dictionary<string, string> fileNamesDict, IEDocsDelayedSaver eDocsSaver)
		{
			var docManagerInfo = businessObject.DocManagerInfo;
			var eDocs = docManagerInfo.GetRelatedEDocs();

			foreach (StorageDocsBase doc in eDocs)
			{
				var oldFileName = doc.SC_FileName;
				if (fileNamesDict.TryGetValue(oldFileName, out var newFileName))
				{
					doc.SC_FileName = newFileName;
					if (eDocsSaver == null)
					{
						throw new InvalidOperationException("You must set an EDocsSaver before trying to save to eDocs");
					}
					eDocsSaver.QueueForSaving(docManagerInfo);
				}
			}
		}
		public static Dictionary<string, string> FileNamesDictExport(string mrn, string oldCSVClearance) => new Dictionary<string, string>()
																					{ { mrn + DocumentCaptureRequestFileNameSuffixes.ExportClearanceDoc, mrn + DocumentCaptureRequestFileNameSuffixes.ExportClearanceDoc + "_OLD_" + oldCSVClearance },
																						{ mrn + DocumentCaptureRequestFileNameSuffixes.ExportT2LFDoc, mrn + DocumentCaptureRequestFileNameSuffixes.ExportT2LFDoc + "_OLD_" + oldCSVClearance } };

		public static Dictionary<string, string> FileNamesDictT2LExpeditionAmendment(string mrn, string oldCSVClearance) => new Dictionary<string, string>()
																					{ { mrn + DocumentCaptureRequestFileNameSuffixes.T2LExpeditionClearanceDoc, mrn + DocumentCaptureRequestFileNameSuffixes.T2LExpeditionClearanceDoc + "_OLD_" + oldCSVClearance } };

		public static Dictionary<string, string> FileNamesDictImport(string mrn, string oldCSVClearance) => new Dictionary<string, string>()
																					{ { mrn + DocumentCaptureRequestFileNameSuffixes.ImportClearanceDoc, mrn + DocumentCaptureRequestFileNameSuffixes.ImportClearanceDoc + "_OLD_" + oldCSVClearance },
																						{ mrn + DocumentCaptureRequestFileNameSuffixes.ImportCertificateDoc, mrn + DocumentCaptureRequestFileNameSuffixes.ImportCertificateDoc + "_OLD_" + oldCSVClearance } };

		public static Dictionary<string, string> FileNamesDictArrivalAtExit(string mrn, string oldCSVClearance) => new Dictionary<string, string>()
																					{ { mrn + DocumentCaptureRequestFileNameSuffixes.ArrivalAtExitDoc, mrn + DocumentCaptureRequestFileNameSuffixes.ArrivalAtExitDoc + "_OLD_" + oldCSVClearance } };

		public static Dictionary<string, string> FileNamesDictEXS(string mrn, string oldCSVClearance) => new Dictionary<string, string>()
																					{ { mrn + DocumentCaptureRequestFileNameSuffixes.EXSClearanceDoc, mrn + DocumentCaptureRequestFileNameSuffixes.EXSClearanceDoc + "_OLD_" + oldCSVClearance } };

		public static Dictionary<string, string> FileNamesDictDVD(string mrn, string oldCSVClearance) => new Dictionary<string, string>()
																					{ { mrn + DocumentCaptureRequestFileNameSuffixes.DVDClearanceDoc, mrn + DocumentCaptureRequestFileNameSuffixes.DVDClearanceDoc + "_OLD_" + oldCSVClearance } };

		static Dictionary<string, string> SpecificFileNamesDictAES(string mrn, string oldCSVClearance) => new Dictionary<string, string>()
																					{ { mrn + DocumentCaptureRequestFileNameSuffixes.ExportAESCertificateEffectiveDepartureDoc, mrn + DocumentCaptureRequestFileNameSuffixes.ExportAESCertificateEffectiveDepartureDoc + "_OLD_" + oldCSVClearance },
																						{ mrn + DocumentCaptureRequestFileNameSuffixes.ExportAccompanyingDoc, mrn + DocumentCaptureRequestFileNameSuffixes.ExportAccompanyingDoc + "_OLD_" + oldCSVClearance } };

		public static Dictionary<string, string> FileNamesDictExportAES(string mrn, string oldCSVClearance) => ZipDictionaries(FileNamesDictExport(mrn, oldCSVClearance), SpecificFileNamesDictAES(mrn, oldCSVClearance));

		static Dictionary<string, string> ZipDictionaries(IDictionary<string, string> first, IDictionary<string, string> second) => first.Concat(second).GroupBy(i => i.Key).ToDictionary(group => group.Key, group => group.First().Value);

		public static Dictionary<string, string> FileNamesDictT2C(string mrn, string oldCSVClearance) => new Dictionary<string, string>()
																					{ { mrn + DocumentCaptureRequestFileNameSuffixes.T2LClearanceDoc, mrn + DocumentCaptureRequestFileNameSuffixes.T2LClearanceDoc + "_OLD_" + oldCSVClearance } };

		public static Dictionary<string, string> FileNamesDictT2LReception(string mrn, string oldCSVClearance) => new Dictionary<string, string>()
																					{ { mrn + DocumentCaptureRequestFileNameSuffixes.T2LReceptionClearanceDoc, mrn + DocumentCaptureRequestFileNameSuffixes.T2LReceptionClearanceDoc + "_OLD_" + oldCSVClearance } };
	}
}
