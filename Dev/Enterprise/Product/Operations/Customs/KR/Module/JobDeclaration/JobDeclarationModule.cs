using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.KR.Messaging.Constants;
using EDIInterchange = Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.KR.Module
{
	public class JobDeclarationModule : Customs.Module.JobDeclarationModule
	{
		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			if (ShowAddInboundMessage)
			{
				result.Add(new ZMenuItem(Res.GetString("88600CF8-5896-40B5-AEB7-2408FB47FC56", "Add Inbound Customs Message"), CustomsMessageImport_Click));
				result.Add(new ZMenuItem(Res.GetString("28B7DDDF-4025-473F-A67A-7F93FB906B6A", "Add Inbound RSP Customs Interchange"), CustomsInterchangeImport_Click));
				result.Add(new ZMenuItem(Res.GetString("5194F9C6-1C9F-49A2-BB75-4EFAA7EFA99A", "Add Inbound DLT Customs Interchange"), CustomsInterchangeDLT_Click));
			}
			return result.ToArray();
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			if (NewMenuItem != null)
			{
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("D2A72F5D-16B7-4D9F-96C1-174BA17A8C7E", "Declaration"), NewDeclarationMenuItem_Click));
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("519832E6-9D94-46AE-AB16-61591B2E1DC0", "Personal Items Declaration (008)"), NewPersonalItemDeclarationMenuItem_Click));
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("3EBEAFFE-01C8-4C6E-937A-03E61A57657F", "Carnet Temporary Import Certificate (D87)"), NewCarnetTemporaryImportCertificateMenuItem_Click));
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("5FFF5528-B097-449D-A574-15E6334B49DC", "Valuation Declaration Template (5SM)"), NewValuationDeclarationTemplate_Click));
			}
			return menuItems.ToArray();
		}

		void NewDeclarationMenuItem_Click(object sender, EventArgs e)
		{
			ShowNewForm();
		}

		void NewPersonalItemDeclarationMenuItem_Click(object sender, EventArgs e)
		{
			ShowNewMiscRequestForm(ElectronicDocumentTypeList.Codes._008);
		}

		void NewCarnetTemporaryImportCertificateMenuItem_Click(object sender, EventArgs e)
		{
			ShowNewMiscRequestForm(ElectronicDocumentTypeList.Codes._D87);
		}

		void NewValuationDeclarationTemplate_Click(object sender, EventArgs e)
		{
			ShowNewMiscRequestForm(ElectronicDocumentTypeList.Codes._5SM);
		}

		void ShowNewMiscRequestForm(string messageType)
		{
			((JobDeclarationController)GetControllerForStandAlone()).ShowMiscDeclarationForm(messageType);
		}

		bool ShowAddInboundMessage
		{
			get { return GlbStaff.CurrentUser.IsSupportUser; }
		}

		void CustomsMessageImport_Click(object sender, EventArgs e)
		{
			using (var stream = GetFileStream())
			{
				if (stream != null)
				{
					try
					{
						var message = CreateEDIMessage(stream);
						try
						{
							Factory.Save();
							Globals.Message.ShowInformation(Res.GetString("BD5B8562-77AB-4710-95CB-6ABF1CAD93FA", "A message was saved with message number [{0}].", message.EM_MessageNum));
						}
						catch (ZSaveException exception)
						{
							ZExceptionReporting.HandleSaveException(exception);
						}
					}
					catch (XmlException exception)
					{
						Globals.Message.ShowError(exception.Message);
					}
				}
			}
		}

		void CustomsInterchangeImport_Click(object sender, EventArgs e)
		{
			InterchangeImport(EDIInterchangeType.RSP);
		}

		void CustomsInterchangeDLT_Click(object sender, EventArgs e)
		{
			InterchangeImport(EDIInterchangeType.DLT);
		}

		void InterchangeImport(string interchangeType)
		{
			using (var stream = GetFileStream())
			{
				if (stream != null)
				{
					try
					{
						var interchange = CreateEDIInterchange(stream, interchangeType);
						try
						{
							Factory.Save();
							Globals.Message.ShowInformation(Res.GetString("CD5B8562-77AB-4710-95CB-6ABF1CAD93FA", "An interchange  was saved with interchange number [{0}].", interchange.EI_InterchangeNum));
						}
						catch (ZSaveException exception)
						{
							ZExceptionReporting.HandleSaveException(exception);
						}
					}
					catch (XmlException exception)
					{
						Globals.Message.ShowError(exception.Message);
					}
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		public static class XmlElementTag
		{
			public const string Root = "Response";
			public const string MessageType = "TypeCode";
			public const string Declaration = "Declaration";
		}

		#endregion

		EDIMessage CreateEDIMessage(Stream stream)
		{
			var firstElement = false;
			var electronicDocumentType = ZString.Empty;

			try
			{
				using (var reader = XmlReader.Create(stream))
				{
					while (reader.Read())
					{
						if (reader.NodeType == XmlNodeType.Element)
						{
							if (!firstElement)
							{
								firstElement = true;
								if (reader.LocalName != XmlElementTag.Root)
								{
									throw new XmlException(Res.GetString("AFC1349A-96E1-4968-8557-AD37A041249A", "Expected Root Element Tag to be <Response>, but found <{0}> instead.", reader.LocalName));
								}
							}
							else if (reader.LocalName == XmlElementTag.MessageType)
							{
								reader.Read();
								if (reader.NodeType == XmlNodeType.Text)
								{
									electronicDocumentType = reader.Value;
								}
								break;
							}
						}
					}
				}
				if (electronicDocumentType.IsEmpty)
				{
					throw new XmlException(Res.GetString("ADEEBCBB-C555-4079-AF9E-6D8127BB0D24", "<TypeCode> element of the response does not exist."));
				}
				else if (!electronicDocumentType.StartsWith("GOVCBR") || electronicDocumentType.Length != 9)
				{
					throw new XmlException(Res.GetString("3546F8FF-CBE7-4BC8-A91B-348D5B8151CB", "Invalid Message Type"));
				}
			}
			catch (Exception)
			{
				throw;
			}

			var message = Factory.New<EDIMessage>();
			message.SetEM_MessageTextOrDataSource(stream);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageType = electronicDocumentType.SubstringSafe(6);
			return message;
		}

		EDIInterchange CreateEDIInterchange(Stream stream, string interchangeType)
		{
			var result = Factory.New<EDIInterchange>();
			result.EI_ApplicationCode = EDIInterchange.ApplicationCodes.KRCustoms;
			result.SetEI_BodyDataSource(new StreamSource(stream));
			result.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			result.EI_Status = EDIInterchange.Status.Queued;
			result.EI_InterchangeType = interchangeType;
			result.EI_From = "KRCustoms";
			result.EI_To = (NoResString)"Odyssey";
			return result;
		}

		protected virtual Stream GetFileStream()
		{
			using (var openFileDialog = new ZOpenFileDialog())
			{
				openFileDialog.CheckFileExists = true;
				openFileDialog.DefaultExt = ".xml";
				openFileDialog.Filter = (NoResString)"XML Data Files (*.xml)|*.xml";
				openFileDialog.Multiselect = false;
				return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.OpenFile() : null;
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
	}
}
