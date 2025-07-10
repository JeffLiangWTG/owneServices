using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Customs.GB.EMCS.Business.PreValidateTraderInfo;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class PreValidateTraderSendingObject : IPreValidateTraderDataProvider
	{
		public PreValidateTraderSendingObject(EU.EMCS.Business.EMCSJobDeclaration declaration, TraderData trader, string productCodes)
		{
			this.declaration = declaration;
			this.trader = trader;
			this.productCodes = productCodes;
		}

		IDataContextDataObject IPreValidateTraderDataProvider.GetDataContext()
		{
			var context = DataContextFactory.New(UniversalXmlInfo.Namespace_2011_11);
			var jobNumber = declaration?.JobNumber ?? string.Empty;
			if (!jobNumber.IsEmpty())
			{
				context.AddDataSource(DataContextType.EMCSJobDeclaration, jobNumber);
			}
			context.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			return context;
		}

		List<Context> IPreValidateTraderDataProvider.GetContextCollection()
		{
			var preValidateTraderBody = PreValidateTraderRequest.CreateRequest(trader, productCodes);
			var encodedPreValidateTraderBody = Convert.ToBase64String(Encoding.UTF8.GetBytes(preValidateTraderBody.SerializeAsJson()));
			return new List<Context>()
			{
				new Context { Type = PreValidateTraderHelper.Constants.ContextTypes.PreValidateTraderBody, Value = encodedPreValidateTraderBody },
				new Context { Type = PreValidateTraderHelper.Constants.Key, Value = $"{declaration?.JE_CustomsProfile}" }
			};
		}

		readonly EU.EMCS.Business.EMCSJobDeclaration declaration;
		readonly TraderData trader;
		readonly string productCodes;
	}
}

