using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.GUI
{
	public partial class CopyAndSendToCustomsForm : ZChildForm
	{
		public CopyAndSendToCustomsForm(JobDeclaration jobDeclarationOriginal, CopyAndSendToCustomsFormData parent) : base(parent)
		{
			this.jobDeclarationOriginal = jobDeclarationOriginal;
			this.parent = parent;
		}
		public override string FormHeading => Res.GetString("BE934D9B-D3D9-4A17-AB93-DA3618DBCB78", "Copy and Submit To Customs");

		readonly CopyAndSendToCustomsFormData parent;
		readonly JobDeclaration jobDeclarationOriginal;

		public static void ShowForm(JobDeclaration jobDeclarationOriginal)
		{
			if (CheckHasEntries(jobDeclarationOriginal))
			{
				CopyAndSendToCustomsFormData formData = new CopyAndSendToCustomsFormData();
				var copyAndSendToCustomsForm = new CopyAndSendToCustomsForm(jobDeclarationOriginal, formData);
				using (var messageSendingForm = copyAndSendToCustomsForm)
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
					{
						copyAndSendToCustomsForm.CopyAndSendToCustomsIfRequested();
					}
				}
			}
		}

		static bool CheckHasEntries(JobDeclaration jobDeclarationOriginal)
		{
			bool hasEntries = true;
			if (!jobDeclarationOriginal.ActiveEntryHeaders.Any())
			{
				Globals.Message.ShowInformation(Res.GetString("2803F169-F926-4D7E-9C5D-1148E74BF02C", "No entries exist – Please generate entries before attempting to send a message to customs."));
				hasEntries = false;
			}
			return hasEntries;
		}

		public void CopyAndSendToCustomsIfRequested()
		{
			using (var progressForm = new ProgressForm())
			{
				var numberOfCopies = parent.NumberOfCopies;
				var numberOfInvoiceLinesCopies = parent.NumberOfInvoiceLinesCopies;
				progressForm.SetStatusAndPercentComplete(Res.GetString("{8612CC30-4A19-416D-B4E2-97C241D7DAB2}", "Copying Declaration..."), 0);
				progressForm.ShowModalTo(this);
				var sendToCustoms = parent.SendToCustoms;
				var count = 0;
				var messageSentPKs = new List<ZGuid>();
				foreach (var copiedDeclaration in CopyAndSendToCustomsJobDeclarationCopier.CreateCopy(numberOfCopies, numberOfInvoiceLinesCopies, jobDeclarationOriginal))
				{
					count++;
					var percentComplete = count * 100 / numberOfCopies;
					if (percentComplete > 100)
					{
						percentComplete = 100;
					}
					progressForm.SetStatusAndPercentComplete(Res.GetString("{BAB9B6E9-BEB0-4103-8806-4229BFCBEB3C}", "Creating {0} of {1} jobs.", count, numberOfCopies), percentComplete);

					if (sendToCustoms)
					{
						progressForm.SetStatusAndPercentComplete(Res.GetString("{E3A8CD6F-300C-4D66-A218-C216170DD71A}", "Creating message(s) for {0}.", copiedDeclaration.JE_DeclarationReference), percentComplete);
						messageSentPKs.AddRange(SendToCustoms(copiedDeclaration));
					}
				}
				if (messageSentPKs.Count > 0)
				{
					progressForm.SetStatusAndPercentComplete(Res.GetString("{4B2D8191-48CC-46F7-BD01-BD936910C5D6}", "Setting messages status to 'QUE'"), 100);
					var factory = new BusinessObjectFactory() { RefreshEnabled = false };
					factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.PK, messageSentPKs)).ForEach(message =>
					{
						message.EM_Status = EDIMessage.Status.Queued;
					});
					try
					{
						factory.Save();
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			if (!parent.HasErrors)
			{
				this.DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}

		IEnumerable<ZGuid> SendToCustoms(JobDeclaration jobDeclaration)
		{
			jobDeclaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var sendingParent = new CopyAndSendToCustomsSendingActionParent(jobDeclaration, parent.MessageType);
			var messageSentPKs = new List<ZGuid>();
			foreach (var action in sendingParent.SendingObjectsCollection.Cast<CopyAndSendToCustomsSendingAction>())
			{
				var sender = action.CreateSender();
				var message = sender.Send();
				message.EM_Status = OutboundEDIMessage.Status.Discarded;
				messageSentPKs.Add(message.PK);
			}
			var wasSaved = false;
			if (messageSentPKs.Count > 0)
			{
				try
				{
					jobDeclaration.Factory.Save();
					wasSaved = true;
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
			return wasSaved ? messageSentPKs.ToArray() : Enumerable.Empty<ZGuid>();
		}
	}
}
