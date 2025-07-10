using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.DFD.Business.Import
{
	class USSupplierDataImporter : DataImporter
	{
		#region Override

		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			additionalTransactionActions = System.Array.Empty<ITransactionParticipant>();
			int numberOfRowsToSave = 0;

			string line = dataReader.ReadLine();
			while ((line = dataReader.ReadLine()) != null)
			{
				CreateOrganisation(line);
				numberOfRowsToSave++;
				if (numberOfRowsToSave % 200 == 0)
				{
					FactoryProvider.SaveCurrentAndCreateNew();
				}
			}

			notifications.Notify(new InfoNotification(string.Format("{0} organisations created.", numberOfRowsToSave)));
			return numberOfRowsToSave > 0;
		}

		#endregion

		#region Implementation

		void CreateOrganisation(string line)
		{
			var dataRow = new USSupplierDataRow(new FlatFileDataRow(new OCsvLine(line).FieldValues));
			var org = FactoryProvider.Current.New<OrgHeader>();
			org.OH_FullName = dataRow.MFName;
			if (!dataRow.ManufacturerID.IsEmpty)
			{
				org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, dataRow.ManufacturerID, RefCountry.LoadFromCountryCode(FactoryProvider.Current, Core.Constants.CountryCodes.UnitedStates));
			}

			org.MainAddress.OA_Address1 = dataRow.MFADDR;
			org.MainAddress.OA_City = dataRow.MFCITY;
			org.MainAddress.OA_PostCode = dataRow.MFZIP;
			org.OH_RL_NKClosestPort = GetPortCode(dataRow);
			org.OH_IsActive = true;
			org.OH_IsConsignor = true;
			org.OH_Language = Core.Constants.Languages.English;
			var generator = new OrgCodeGenerator();
			org.OH_Code = generator.GenerateCode(new USSupplierOrgCodeInfo(org), org.Factory).GetProposedCode();
		}

		ZString GetPortCode(USSupplierDataRow dataRow)
		{
			var query = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, dataRow.MFCTCD);
			query.AddToFilter(RefUNLOCOSchema.RL_PortName, dataRow.MFCITY);
			var unlocos = FactoryProvider.Current.Load<RefUNLOCO>(query);
			if (unlocos.Length > 0)
			{
				return unlocos[0].RL_Code;
			}

			return string.Concat(dataRow.MFCTCD, "ZZZ");
		}

		#endregion
	}
}
