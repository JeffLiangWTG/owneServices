using Enterprise.Customs.GB.MCP;
using Enterprise.MailManager.MailFilters;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

#region RRA12
[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.GB.MCP.ServiceTasks.RRA12.RRA12ServiceTask.Code,
	MailDBItemsSchema.Constants.TableName,
	new[]
	{
		MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.GbMcpRra12,
		MailDBItemsSchema.Constants.MI_Status      + "=" + MailManager.StatusCodeList.Codes.Queued,
		MailDBItemsSchema.Constants.MI_Direction   + "=" + MailManager.DirectionList.Codes.Receive
	},
	"MCP Destin8 RRA12 mail inbound"
	)]
#endregion

#region RRA01 and RRA11 and RRA06

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11.McpStatusRetrieverServiceProvider.Code,
	MailDBItemsSchema.Constants.TableName,
	new[]
	{
		MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.GbMcpRra11AndRra06AndRra01,
		MailDBItemsSchema.Constants.MI_Status      + "=" + MailManager.StatusCodeList.Codes.Queued,
		MailDBItemsSchema.Constants.MI_Direction   + "=" + MailManager.DirectionList.Codes.Receive
	},
	"Destin8 RRA11/06/01 mail inbound")]
#endregion

#region PHS11

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Messaging.Integration.ApplicationCodeList.Codes.GbMcpPortHealth,
	MailDBItemsSchema.Constants.TableName,
	new[]
	{
		MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.GbMcpPHS,
		MailDBItemsSchema.Constants.MI_Status      + "=" + MailManager.StatusCodeList.Codes.Queued,
		MailDBItemsSchema.Constants.MI_Direction   + "=" + MailManager.DirectionList.Codes.Receive
	},
	"Destin8 Port Health Status mail inbound")]
#endregion

#region MCM
[assembly: HostedServiceBusinessObjectBinding(Constants.ServiceTasksCode.MiscTextAndIslServiceTaskCode,
	MailDBItemsSchema.Constants.TableName,
	new[]
	{ MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.GbMcpMiscTextAndEmails,
		MailDBItemsSchema.Constants.MI_Status      + "=" + MailManager.StatusCodeList.Codes.Queued,
		MailDBItemsSchema.Constants.MI_Direction   + "=" + MailManager.DirectionList.Codes.Receive
	},
	"Destin8 Misc mail inbound")]
#endregion
