using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public enum DigitalCertificateLoadButtonText
	{
		Default = 0,
		Load,
		Add
	}

	[DefaultBindingProperty("FileDataAsString")]
	public partial class DigitalCertificateControl_p12 : ZUserControl, IExtendedControl
	{
		public Control Host => this;

		public IControlExtensionCollection Extensions { get; }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		protected virtual IEnumerable<string> SupportingFileExtensionList { get; } = new[] { ".P12", ".PFX" };

		bool IsExtensionInTheSupportingFileExtensionList(string extension) => SupportingFileExtensionList.Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase));

		#region Init

		public DigitalCertificateControl_p12()
		{
			InitializeComponent();
			State = DataLoadState.NoData;
			Extensions = new DefaultControlExtensionCollection(this);
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

		#endregion

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
		public byte[] FileDataAsBinary()
		{
			return fBinaryFileData;
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
				FileDataAsStringChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		[Browsable(true), DefaultValue(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool AllowClear
		{
			get { return ClearButton.Visible; }
			set { ClearButton.Visible = value; }
		}

		[Browsable(true), DefaultValue(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool ShowUserFeedbackLabel
		{
			get => UserFeedbackLabel.Visible;
			set => UserFeedbackLabel.Visible = value;
		}

		[Browsable(true), DefaultValue(DigitalCertificateLoadButtonText.Default), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public DigitalCertificateLoadButtonText LoadButtonText
		{
			get => fLoadButtonText;
			set
			{
				fLoadButtonText = value;
				switch (value)
				{
					case DigitalCertificateLoadButtonText.Add:
						LoadButton.CaptionResourceString = Res.GetData("2aa1fb7f-3bd1-4ac5-9008-df75fe3591f5", "Add");
						break;

					default:
						LoadButton.CaptionResourceString = Res.GetData("67cd616b-c9d4-4ee4-b3a5-aa3df45f6da0", "Load");
						break;
				}
			}
		}
		DigitalCertificateLoadButtonText fLoadButtonText = DigitalCertificateLoadButtonText.Default;

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public int? DaysBeforeExpiryWarningMessage { get; set; }

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

		[Browsable(true)]
		public event EventHandler OnDataView
		{
			add { fOnDataView += value; }
			remove { fOnDataView -= value; }
		}

		public event EventHandler FileDataAsStringChanged;

		#endregion

		#region Show Certificate Details

		void ViewButton_Click(object sender, EventArgs e)
		{
			fOnDataView?.Invoke(this, new EventArgs());
			ShowCertificateDetails();
		}

		void ShowCertificateDetails()
		{
			if (!IsBinaryFileDataEmpty)
			{
				try
				{
					if (UpdateCertificate())
					{
						var san = ParseSubjectAlternativeNames(Cert);
						Globals.Message.ShowInformation(
							(NoResString)"Issued To: " + Cert.Subject + (NoResString)"\n" +
							(NoResString)"Issued By: " + Cert.Issuer + (NoResString)"\n\n" +

							(NoResString)"Valid From: " + Cert.NotBefore + (NoResString)"\n" +
							(NoResString)"Valid To: " + Cert.NotAfter + (NoResString)"\n\n" +

							(NoResString)"Serial Number: " + Cert.SerialNumber + (NoResString)"\n\n" +

							(string.IsNullOrEmpty(san) ? (NoResString)"" : ((NoResString)"Subject Alternative Names: \n     " + san + (NoResString)"\n\n")) +

							(NoResString)"Errors: " + GetMessagesSeparatedByBreakLine(Errors()) + (NoResString)"\n" +
							(NoResString)"Warnings: " + GetMessagesSeparatedByBreakLine(Warnings()),
							Res.GetString("411E85DA-A83F-4B62-9064-856185BC9483", "Certificate Details"));
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError("ERROR: " + ex.Message);
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("FD47F815-B949-4F47-8037-EE6637075B89", "There is currently no Certificate loaded."));
			}
		}

		static string ParseSubjectAlternativeNames(X509Certificate2 cert)
		{
			var result = string.Empty;
			var san = cert.Extensions?.OfType<X509Extension>().FirstOrDefault(x => x.Oid.FriendlyName == "Subject Alternative Name");
			if (san != null)
			{
				result = (new AsnEncodedData(san.Oid, san.RawData)).Format(true);
			}
			return result;
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
			return (string.IsNullOrEmpty(result)) ? Res.GetString("3f8fd924-d102-40bf-87cd-dc121c1ebeda", "None") : result;
		}

		public string[] Errors()
		{
			var timeToVerifyAgainst = ZDateTime.Now;
			ArrayList errorList = new ArrayList();

			if (timeToVerifyAgainst > Cert.NotAfter)
			{
				errorList.Add(Res.GetString("592F1DD2-3B23-4C9E-9AD1-7413E174B684", "The certificate has expired as of {0}.", Cert.NotAfter));
			}
			if (timeToVerifyAgainst < Cert.NotBefore)
			{
				errorList.Add(Res.GetString("A780E838-45EA-4893-8EAE-2795851873CC", "The certificate is not yet valid.  The certificate will become valid at {0}.", Cert.NotBefore));
			}

			return (string[])errorList.ToArray(typeof(string));
		}

		public string[] Warnings()
		{
			var warningList = new List<string>();
			var certificateWillExpireWarning = GetCertificateWillExpireWarning();

			if (!string.IsNullOrEmpty(certificateWillExpireWarning))
			{
				warningList.Add(certificateWillExpireWarning);
			}

			return warningList.ToArray();
		}

		string GetCertificateWillExpireWarning()
		{
			var result = string.Empty;
			var timeToVerifyAgainst = ZDateTime.Now;

			if (Cert != null && timeToVerifyAgainst <= Cert.NotAfter && GetCertificateAboutToExpireThreshold(timeToVerifyAgainst) > Cert.NotAfter)
			{
				result = GetCertificateAboutToExpireMessage(Cert.NotAfter);
			}

			return result;
		}

		protected virtual ZDateTime GetCertificateAboutToExpireThreshold(ZDateTime timeToVerifyAgainst) => timeToVerifyAgainst.AddDays(DaysBeforeExpiryWarningMessage.GetValueOrDefault(7));
		protected virtual string GetCertificateAboutToExpireMessage(DateTime expiryDate) => Res.GetString("90F5B0EF-CD7B-4BE5-A2E5-49E77C602061", "The certificate will shortly expire.  Please replace this certificate by {0}.", expiryDate);

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
		protected byte[] fBinaryCertificateData;
		protected string fInitialDirectory;

		EventHandler fDataLoaded;
		EventHandler fDataCleared;
		EventHandler fOnDataView;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				components?.Dispose();
				FileDialog?.Dispose();
				Cert?.Dispose();
				Crypt?.Dispose();
				Extensions.Dispose();
			}
			base.Dispose(isNotFinalizing);
		}

		void LoadButton_Click(object sender, EventArgs e)
		{
			DialogResult result = FileDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				if (IsExtensionInTheSupportingFileExtensionList(Path.GetExtension(FileDialog.UnmappedFileName)))
				{
					using (var stream = FileDialog.OpenFile())
					{
						fBinaryFileData = stream.ToByteArray();
					}

					if (IsValidCertificateFile(fBinaryFileData))
					{
						State = DataLoadState.DataExists;

						fDataLoaded?.Invoke(this, EventArgs.Empty);
						FileDataAsStringChanged?.Invoke(this, EventArgs.Empty);
					}
					else
					{
						Globals.Message.ShowWarning(Res.GetString("13855cc8-3f36-47b7-a095-f93023497db7", "Invalid Data. Data not loaded."));
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("c4640b64-de3b-4d87-82c7-30e62b4ffa2f", "Invalid file type. Data not loaded."));
				}
			}
		}

		protected X509Certificate2 Cert;
		RSA Crypt;
		public string CertificatePassword;

		protected bool IsValidCertificateFile(byte[] data)
		{
			return data.Length > 0;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Exception Text")]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Text")]
		public bool UpdateCertificate()
		{
			var result = false;
			try
			{
				Cert = new X509Certificate2(fBinaryFileData, CertificatePassword,
					X509KeyStorageFlags.MachineKeySet);

				Crypt = Cert.GetRSAPrivateKey();

				fBinaryCertificateData = StringToBinary(Cert.ToString());

				result = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ex.Message == "The specified network password is not correct.\r\n")
				{
					Globals.Message.ShowError("ERROR: The specified certificate password is not correct.\r\n");
				}
				else
				{
					Globals.Message.ShowError("ERROR: " + ex.Message);
				}
			}

			return result;
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			fBinaryFileData = null;
			State = DataLoadState.DataCleared;

			fDataCleared?.Invoke(this, new EventArgs());
			FileDataAsStringChanged?.Invoke(this, EventArgs.Empty);
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

	}
}
