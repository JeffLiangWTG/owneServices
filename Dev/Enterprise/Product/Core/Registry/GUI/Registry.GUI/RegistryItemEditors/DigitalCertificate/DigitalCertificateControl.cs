using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.CryptoUtilities;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public enum DataLoadState
	{
		Unknown,
		NoData,
		DataExists,
		DataCleared
	}

	[DefaultBindingProperty("FileDataAsString")]
	public partial class DigitalCertificateControl : ZUserControl
	{
		[Obsolete("Use the constructor that takes a FileUpLoaderX509CertificateRegistryEditorInfo, this constructor is just for the designer", true)]
		public DigitalCertificateControl()
		{
			InitializeComponent();
		}

		public DigitalCertificateControl(FileUpLoaderX509CertificateRegistryEditorInfo editorInfo)
		{
			this.EditorInfo = editorInfo;
			InitializeComponent();
			State = DataLoadState.NoData;
		}

		public void SetFileData(byte[] value)
		{
			fBinaryFileData = value;

			if (!IsBinaryFileDataEmpty)
			{
				State = DataLoadState.DataExists;
			}
			else
			{
				State = DataLoadState.NoData;
			}
		}

		#region Properties

		/// <summary>
		/// Changes user feedback display text
		/// </summary>
		public DataLoadState State
		{
			get { return fState; }
			set
			{
				fState = value;
				switch (value)
				{
					case DataLoadState.NoData:
						UserFeedbackLabel.Text = Res.GetString("ace56b3e-0340-4550-b5e0-714ac572516d", "No Data");
						break;

					case DataLoadState.DataExists:
						UserFeedbackLabel.Text = Res.GetString("d48eb460-ede8-486f-8329-c212dc4bc77f", "Data Exists");
						break;

					case DataLoadState.DataCleared:
						UserFeedbackLabel.Text = Res.GetString("f66fae0b-03b6-4bef-a85e-20dd65421479", "Data Cleared");
						break;

					default:
						UserFeedbackLabel.Text = "";
						break;
				}
			}
		}
		protected DataLoadState fState;

		protected bool IsBinaryFileDataEmpty
		{
			get { return (fBinaryFileData == null || fBinaryFileData.Length == 0); }
		}

		/// <summary>
		/// The file filters to display in the dialog, for example, "Key files (*.key)|*.key|All files (*.*)|*.*"
		/// </summary>
		[Browsable(true)]
		public string FileFilter
		{
			get { return FileDialog.Filter; }
			set { FileDialog.Filter = value; }
		}

		[Browsable(true)]
		public string FileDialogTitle
		{
			get { return FileDialog.Title; }
			set { FileDialog.Title = value; }
		}

		/// <summary>
		/// InitialDirectory for Open File Dialog
		/// </summary>
		public string InitialDirectory
		{
			get { return FileDialog.InitialDirectory; }
			set { FileDialog.InitialDirectory = value; }
		}

		/// <summary>
		/// File data in unconverted format
		/// </summary>
		public byte[] FileDataAsBinary
		{
			get { return fBinaryFileData; }
		}

		/// <summary>
		/// File data converted to string format
		/// </summary>
		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnly")]
		public string FileDataAsString
		{
			get { return BinaryToString(fBinaryFileData); }
			set
			{
				fBinaryFileData = StringToBinary(value);
				State = (value.Length > 0) ? DataLoadState.DataExists : DataLoadState.NoData;
				RaiseFileDataAsStringChanged();
			}
		}

		[Browsable(true), DefaultValue(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool AllowClear
		{
			get { return ClearButton.Visible; }
			set { ClearButton.Visible = value; }
		}

		#endregion

		#region Events

		[Browsable(true)]
		public event EventHandler DataLoaded
		{
			add { fDataLoaded += value; }
			remove { fDataLoaded -= value; }
		}

		[Browsable(true)]
		public event EventHandler DataCleared
		{
			add { fDataCleared += value; }
			remove { fDataCleared -= value; }
		}

		public event EventHandler FileDataAsStringChanged;

		#endregion

		#region Show Certificate Details

		void ViewButton_Click(object sender, EventArgs e)
		{
			ShowCertificateDetails();
		}

		void ShowCertificateDetails()
		{
			if (!IsBinaryFileDataEmpty)
			{
				ICryptoContainer certificate = null;

				try
				{
					certificate = EditorInfo.GetCertificate(FileDataAsBinary);

					string emailAddress = string.IsNullOrEmpty(certificate.EmailAddress) ? Res.GetString("3f8fd924-d102-40bf-87cd-dc121c1ebeda", "None") : certificate.EmailAddress;
					CertificateState certificateState = certificate.GetCertificateState(ZDateTime.Now.ToDateTime());

					Globals.Message.ShowInformation(
						(NoResString)"Issued To: " + certificate.Name + (NoResString)"\n" +
						(NoResString)"Issued By: " + certificate.IssuerName + (NoResString)"\n\n" +
						(NoResString)"Valid From: " + certificate.ValidFromDate + (NoResString)"\n" +
						(NoResString)"Valid To: " + certificate.ValidToDate + (NoResString)"\n\n" +
						(NoResString)"Email Address: " + emailAddress + (NoResString)"\n" +
						(NoResString)"Serial Number: " + certificate.SerialNumber + (NoResString)"\n\n" +
						(NoResString)"Errors: " + GetMessagesSeparatedByBreakLine(certificateState.Errors) + (NoResString)"\n" +
						(NoResString)"Warnings: " + GetMessagesSeparatedByBreakLine(certificateState.Warnings),
						Res.GetString("c897a5ac-53a1-4143-8e4f-b67f5b020515", "Certificate Details"));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError("ERROR: " + ex.Message);
				}
				finally
				{
					IDisposable disposableCertificate = certificate as IDisposable;

					if (disposableCertificate != null)
					{
						disposableCertificate.Dispose();
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("3ff1447b-9170-4a9f-bc0e-f1f32476f0fe", "There is currently no Certificate selected."));
			}
		}

		static string GetMessagesSeparatedByBreakLine(string[] messages)
		{
			string result = "";

			if (messages.Length > 0)
			{
				result = messages[0];

				for (int i = 1; i < messages.Length; i++)
				{
					result += "\n" + messages[i];
				}
			}

			return string.IsNullOrEmpty(result) ? Res.GetString("3f8fd924-d102-40bf-87cd-dc121c1ebeda", "None") : result;
		}

		#endregion

		#region ReadOnly

		public bool ReadOnly
		{
			get { return !LoadButton.Enabled; }
			set
			{
				LoadButton.Enabled = !value;
				ClearButton.Enabled = !value;
			}
		}

		#endregion

		#region Implementation

		protected byte[] fBinaryFileData;
		protected string fInitialDirectory;

		EventHandler fDataLoaded;
		EventHandler fDataCleared;

		readonly FileUpLoaderX509CertificateRegistryEditorInfo EditorInfo;

		void LoadButton_Click(object sender, EventArgs e)
		{
			DialogResult result = FileDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				using (var stream = FileDialog.OpenFile())
				{
					fBinaryFileData = stream.ToByteArray();
				}
				if (IsValidCertificateFile(fBinaryFileData))
				{
					State = DataLoadState.DataExists;

					if (fDataLoaded != null)
					{
						fDataLoaded(this, EventArgs.Empty);
					}

					RaiseFileDataAsStringChanged();

					if (FileDialog.UnmappedFileName.ToUpper().Contains(".CER"))
					{
						try
						{
							Certificate.VerifyCertificateContextFromBytes(fBinaryFileData);
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							Globals.Message.ShowError(Res.GetString("e6dde3ce-1e29-4f77-ad69-c2d2cf1fa4e0", "ERROR: {0}", ex.Message));
						}
					}
				}
				else
				{
					Globals.Message.ShowWarning(Res.GetString("13855cc8-3f36-47b7-a095-f93023497db7", "Invalid Data. Data not loaded."));
				}
			}
		}

		protected void RaiseFileDataAsStringChanged()
		{
			if (FileDataAsStringChanged != null)
			{
				FileDataAsStringChanged(this, EventArgs.Empty);
			}
		}

		protected bool IsValidCertificateFile(byte[] data)
		{
			return data.Length > 0;
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			fBinaryFileData = null;
			State = DataLoadState.DataCleared;
			if (fDataCleared != null)
			{
				fDataCleared(this, new EventArgs());
			}

			RaiseFileDataAsStringChanged();
		}

		static String BinaryToString(byte[] binaryData)
		{
			return (binaryData != null) ? Encoding.ASCII.GetString(binaryData) : "";
		}

		static byte[] StringToBinary(string stringData)
		{
			return Encoding.ASCII.GetBytes(stringData);
		}

		#endregion

		#region IAllowNullableControl Members

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AllowNull
		{
			get { return true; }
			set { }
		}

		#endregion

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (FileDialog != null)
				{
					FileDialog.Dispose();
				}
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
