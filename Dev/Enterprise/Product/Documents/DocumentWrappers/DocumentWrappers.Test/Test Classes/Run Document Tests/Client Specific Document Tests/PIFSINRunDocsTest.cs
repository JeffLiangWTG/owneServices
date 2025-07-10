using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class PIFSINRunDocsTest : ClientSpecificRunDocsTest
	{
		#region Overrides
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		protected override ZString ClientName
		{
			get { return "PIF"; }
		}

		#endregion

		[ExpectNoExceptions()]
		public void TestPhoenixBill()
		{
			SetShipmentHBLType("PIF");
			DocumentCommand command;
			RunDocument("Bill Of Lading", out command);
			AssertBillOfLadingPivotsHaveDocType(command);
		}

		void AssertBillOfLadingPivotsHaveDocType(DocumentCommand command)
		{
			ZString errorMessage = ZString.Empty;
			foreach (DocumentEngine.Business.StmMenuTemplatePivotBase pivot in command.Documents)
			{
				if (pivot.SI_RT_DocType.IsEmpty)
				{
					errorMessage += pivot.SI_DocumentTitle + " of " + command.SU_MenuName + " doesn't have a DocType (SI_RT_DocType) specified.";
					errorMessage += System.Environment.NewLine;
				}
			}
			if (!errorMessage.IsEmpty)
			{
				Fail(errorMessage.ToString());
			}
		}
	}
}
