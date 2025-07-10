using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class TransactionBatchXmlDataImportForm : DataImporterForm
	{
		public TransactionBatchXmlDataImportForm()
			: base(Res.GetString("d9d5e651-0339-4068-8e56-79e2efb7cdc1", "Import Transaction Batch XML Files"), null)
		{
			InitializeComponent();
		}

		void ImportXmlFilesButton_Click(object sender, EventArgs e)
		{
			var files = GetFiles();

			if (files != null && files.Any())
			{
				var fatalErrorOccurred = false;
				var message = Res.GetString("230dc8d1-c1b6-455e-a10b-f6dbf92900ab", "Importing Transaction Batch XML Files.");

				BusinessEntity.ImportOperationDescription = message;

				try
				{
					var factory = new BusinessObjectFactory();

					if (OnBeforeImport())
					{
						var originalCursor = Cursor;

						try
						{
							Cursor = Cursors.WaitCursor;
							ImportFromFiles(factory, files);

							factory.Save();
						}
						finally
						{
							Cursor = originalCursor;
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					fatalErrorOccurred = true;
					BusinessEntity.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
				}
				finally
				{
					OnAfterImport(fatalErrorOccurred);
				}
			}
		}

		protected virtual string[] GetFiles()
		{
			string[] result = null;

			using (var openFileDialog = new ZOpenFileDialog())
			{
				openFileDialog.Filter = "XML Files (*.xml)|*.xml";
				openFileDialog.Multiselect = true;
				openFileDialog.CheckFileExists = true;

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					result = openFileDialog.ForceLocalFiles();
				}
			}

			return result;
		}

		void ImportFromFiles(BusinessObjectFactory factory, string[] files)
		{
			var i = 0;

			foreach (var file in files)
			{
				try
				{
					var messageNumber = string.Concat("ARL", ZDateTime.Now.ToString("yyMMddhhmmss", CultureInfo.InvariantCulture), i.ToString("00", CultureInfo.InvariantCulture));

					var interchange = factory.New<EDIInterchange>();
					interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
					interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CAIMP;
					interchange.EI_GB = GlbBranch.CurrentBranch.PK;
					interchange.EI_To = "ARLMSGTST";
					interchange.EI_From = GlbCompany.CurrentCompany.LicenceEnterpriseCode;
					interchange.EI_InterchangeNum = messageNumber;
					interchange.EI_SystemCreateTimeUtc = ZDateTime.Now;

					var message = interchange.ContainedMessages.AddNew();
					message.EM_GB = GlbBranch.CurrentBranch.PK;
					message.EM_GE = GlbDepartment.CurrentDepartment.PK;
					message.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
					message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
					message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
					message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
					message.EM_MessageText = File.ReadAllText(file);
					message.EM_Status = EDIMessageStatusList.Codes.Queued;
					message.EM_MessageNum = messageNumber;
					message.EM_EI = interchange.PK;
					message.EM_SystemCreateTimeUtc = ZDateTime.Now;

					i++;

					BusinessEntity.RecordsAdded++;

					var messageContent = string.Format(CultureInfo.InvariantCulture, "[Success] Message Number:{0}{1}File:{2}{1}", message.EM_MessageNum, System.Environment.NewLine, file);
					BusinessEntity.Notify(new InfoNotification(messageContent));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var messageContent = string.Format(CultureInfo.InvariantCulture, "[Failed] File:{0}{1}Error:{2}{1}", file, System.Environment.NewLine, ex.Message);
					BusinessEntity.Notify(new WarningNotification(messageContent));
				}
			}
		}
	}
}
