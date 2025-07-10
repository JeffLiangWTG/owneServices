using System.Windows.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CusMiscRequestHeaderViewForm))]
	sealed class CusMiscRequestFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var exportHeader = Factory.New<CusMiscRequestHeader>();
			exportHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
			using (var form = new CusMiscRequestHeaderViewForm(exportHeader))
			{
				AssertEquals("(Exp) Application for Extended Office Hours - ", form.FormCaption);
			}
			var exportCusEntryNum = Factory.New<CusEntryNumber>();
			exportCusEntryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5AC;
			exportCusEntryNum.CE_EntryNum = "40615220000003U";
			exportCusEntryNum.CE_ParentID = exportHeader.PK;
			exportCusEntryNum.CE_ParentTable = CusMiscRequestHeader.Schema.TableName;
			using (var form = new CusMiscRequestHeaderViewForm(exportHeader))
			{
				AssertEquals("(Exp) Application for Extended Office Hours - 40615-22-0000003U", form.FormCaption);
			}

			var importHeader = Factory.New<CusMiscRequestHeader>();
			importHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			using (var form = new CusMiscRequestHeaderViewForm(importHeader))
			{
				AssertEquals("(Imp) Application for Extended Office Hours - ", form.FormCaption);
			}
			var importCusEntryNum = Factory.New<CusEntryNumber>();
			importCusEntryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5GW;
			importCusEntryNum.CE_EntryNum = "40615220000003U";
			importCusEntryNum.CE_ParentID = importHeader.PK;
			importCusEntryNum.CE_ParentTable = CusMiscRequestHeader.Schema.TableName;
			using (var form = new CusMiscRequestHeaderViewForm(importHeader))
			{
				AssertEquals("(Imp) Application for Extended Office Hours - 40615-22-0000003U", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var result = new CusMiscRequestHeaderViewForm(Header);
			result.ControllerID = ControllerIDs.Customs.KR.MiscRequestMessages;
			return result;
		}

		CusMiscRequestHeader Header
		{
			get { return header ?? (header = Factory.New<CusMiscRequestHeader>()); }
		}
		CusMiscRequestHeader header;
	}
}
