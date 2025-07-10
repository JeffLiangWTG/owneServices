using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.Common.GUI
{
	public static class BorderWiseAsyncBatchTariffProcessorProvider
	{
		public static bool ClassifyDeclarationJobInBorderWise(Form parentForm, ICommonInvoiceDataProvider baseJobDeclaration)
		{
			if (!Utilities.IsBorderWiseMultiLineClassificationEnabled)
			{
				return false;
			}

			return new BorderWiseLauncher().ClassifyDeclarationJobInBorderWise(parentForm, baseJobDeclaration);
		}

		public static void SendMessageAndDisposeConnectionIfNeeded(ICommonInvoiceDataProvider baseJobDeclaration, Dictionary<ZGuid, string> invoicePksAction, bool isSave = false)
		{
			if (!Utilities.IsBorderWiseMultiLineClassificationEnabled)
			{
				return;
			}

			new BorderWiseLauncher().SendMessageAndDisposeConnectionIfNeeded(baseJobDeclaration, invoicePksAction, isSave);
		}
	}
}
