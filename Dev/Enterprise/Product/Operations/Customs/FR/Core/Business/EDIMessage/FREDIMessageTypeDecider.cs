using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class FREDIMessageTypeDecider : TypeDecider, Integration.Customs.FR.IFREDIMessageTypeDecider
	{
		public override Type GetTypeForNew() => null;

		public override Type GetTypeForBinding() => null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var messageType = row[EDIMessageSchema.Constants.EM_MessageType].ToString().Trim();
			var messageSubType = row[EDIMessageSchema.Constants.EM_MessageSubType].ToString().Trim();

			switch (messageType)
			{
				case MessageTypeList.Codes.EXC:
					return typeof(DeltaCExportFREDIMessage);
				case MessageTypeList.Codes.IMC:
					return typeof(DeltaCImportFREDIMessage);
				case MessageTypeList.Codes.EXD:
					return typeof(DeltaDExportFREDIMessage);
				case MessageTypeList.Codes.IMD:
					return typeof(DeltaDImportFREDIMessage);
				case MessageTypeList.Codes.DCG:
					return typeof(DCGResponseFREDIMessage);
				case MessageTypeList.Codes.CIN:
					return typeof(CINImportResponseFREDIMessage);
				case MessageTypeList.Codes.ECS:
					switch (messageSubType)
					{
						case MessageSubTypeList.Codes.ARR:
							return typeof(ECSArrivalFREDIMessage);
						case MessageSubTypeList.Codes.DEP:
							return typeof(ECSDepartureFREDIMessage);
						default:
							return typeof(ECSFREDIMessage);
					}
				case MessageTypeList.Codes.DEC:
					return typeof(DeltaIEFREDIMessage);
				case MessageTypeList.Codes.STO:
					return typeof(PNTSEDIMessage);
				case MessageTypeList.Codes.TP5:
					return typeof(NCTSFREDIMessage);
				default:
					return typeof(FREDIMessage);
			}
		}
	}
}
