using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ASYCUDAManifest.Business.UniversalDataTransfer
{
	public class AsycudaBillDataObjectWriter : AsycudaBillDataObjectWriter<AsycudaBill>
	{
		public AsycudaBillDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected override void PopulateSpecificData(AsycudaBill billBO, Shipment uxml)
		{
			base.PopulateSpecificData(billBO, uxml);

			if (billBO.Header.ShowExportGeneralManifest)
			{
				uxml.SetEntryNumberCollection(() =>
				{
					var result = new List<EntryNumber>();
					var sadOfficeCode = billBO.SADOfficeCode;
					if (!sadOfficeCode.IsEmpty)
					{
						result.Add(new EntryNumber
						{
							Type = new EntryType
							{
								Code = Constants.CustomsEntryType.SAD,
								Description = "Export General Manifest Registry Information",
							},
							EntryLineReference = sadOfficeCode,
							Number = billBO.SADRegistrationNumber,
							IssueDate = billBO.SADRegistrationDate,
							Category = billBO.SADRegistrationSerial,
						});
					}
					return result;
				});
			}
		}
	}
}
