using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using eServices.eHubDataModel.eHubTransactionsCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace eServices.eHubPortal.Tests;

internal static class HelperExtensions
{

	internal static eHubTransactionsContext SetupInMemoryDatabase(this TestServiceProvider services, [CallerMemberName] string dbName = "")
	{
		eHubTransactionsContext CreateDbContext() => new eHubTransactionsContext(new DbContextOptionsBuilder<eHubTransactionsContext>()
			.UseInMemoryDatabase(dbName)
			.LogTo(NUnit.Framework.TestContext.WriteLine, LogLevel.Information)
			.ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
			.Options);

		var dbContextFactory = new Mock<IDbContextFactory<eHubTransactionsContext>>();
		dbContextFactory.Setup(x => x.CreateDbContext()).Returns(CreateDbContext);
		dbContextFactory.Setup(x => x.CreateDbContextAsync(default)).ReturnsAsync(CreateDbContext);
		services.AddSingleton<IDbContextFactory<eHubTransactionsContext>>(dbContextFactory.Object);

		return CreateDbContext();
	}

	internal static void LoadInMemoryData(this eHubTransactionsContext context, string path)
	{
		var eHubTransactionsJson = JsonObject.Parse(GetContentFileAsString(path));
		T[] ParseFromJson<T>() => (eHubTransactionsJson![typeof(T).Name]?.ToString() is string source
			? JsonSerializer.Deserialize<T[]>(source) : null) ?? [];

		context.eHubOwner.AddRange(ParseFromJson<eHubOwner>()
			.ExceptBy(context.eHubOwner.Select(x => x.OW_ID), x => x.OW_ID));
		context.eHubClient.AddRange(ParseFromJson<eHubClient>()
			.ExceptBy(context.eHubClient.Select(x => x.CC_PK), x => x.CC_PK));
		context.eHubTransformationSet.AddRange(ParseFromJson<eHubTransformationSet>()
			.ExceptBy(context.eHubTransformationSet.Select(x => x.TS_PK), x => x.TS_PK));
		context.eHubRoutingRule.AddRange(ParseFromJson<eHubRoutingRule>()
			.ExceptBy(context.eHubRoutingRule.Select(x => x.RR_PK), x => x.RR_PK));
		context.eHubCodeSet.AddRange(ParseFromJson<eHubCodeSet>()
			.ExceptBy(context.eHubCodeSet.Select(x => x.CS_PK), x => x.CS_PK));
		context.eHubCodeSetResult.AddRange(ParseFromJson<eHubCodeSetResult>()
			.ExceptBy(context.eHubCodeSetResult.Select(x => x.CR_PK), x => x.CR_PK));
		context.eHubCodeMapKey.AddRange(ParseFromJson<eHubCodeMapKey>()
			.ExceptBy(context.eHubCodeMapKey.Select(x => x.CK_PK), x => x.CK_PK));
		context.eHubCodeMapValue.AddRange(ParseFromJson<eHubCodeMapValue>()
			.ExceptBy(context.eHubCodeMapValue.Select(x => new { x.CV_CR, x.CV_CK }), x => new { x.CV_CR, x.CV_CK }));
		context.SaveChanges();
	}

	internal static string GetContentFileAsString(string path)
	{
		var fullPath = Path.Combine(NUnit.Framework.TestContext.CurrentContext.TestDirectory, path);
		return File.ReadAllText(fullPath);
	}
}
