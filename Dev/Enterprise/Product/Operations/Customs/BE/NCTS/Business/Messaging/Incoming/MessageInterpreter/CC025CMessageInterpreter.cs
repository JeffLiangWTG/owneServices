using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC025CMessageInterpreter : BaseMessageInterpreter<ICC025CDataProvider>
	{
		public override string Interpret(ICC025CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var note = new ZStringBuilder();
			switch (dataProvider.ReleaseIndicator)
			{
				case Constants.ReleaseIndicator.FullRelease:
					note.Append((NoResString)"All Goods are released for transit upon arrival. The movement is closed.");
					break;
				case Constants.ReleaseIndicator.PartialRelease:
					note.Append((NoResString)"Goods are partially released.");
					break;
				case Constants.ReleaseIndicator.PartialReleaseClosed:
					note.Append((NoResString)"Goods are partially released. The movement is closed.");
					break;
				case Constants.ReleaseIndicator.NoRelease:
					note.Append((NoResString)"No release of Goods.");
					break;
			}

			if (dataProvider.ReleaseIndicator != Constants.ReleaseIndicator.NoRelease)
			{
				foreach (var houseConsignment in dataProvider.HouseConsignments)
				{
					note.Append($"{houseConsignment.SequenceNumber}) House Bill: {(houseConsignment.ReleaseType == Constants.ReleaseType.PartialRelease ? (NoResString)"partial release" : (NoResString)"full release")}");
					foreach (var consignmentItem in houseConsignment.ConsignmentItems)
					{
						note.Append($"{consignmentItem.DeclarationSequenceNumber}) Item: {(consignmentItem.ReleaseType == Constants.ReleaseType.PartialRelease ? (NoResString)"partial release" : (NoResString)"full release")}");
						foreach (var package in consignmentItem.Packaging)
						{
							note.Append($"-- {package.NumberOfPackages} {package.TypeOfPackages} with the marks and numbers '{package.ShippingMarks}' are released");
						}
					}
				}
			}

			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
