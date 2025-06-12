using ProcRoll;
using Serilog;

internal class Program
{
	private static async Task Main(string[] args)
	{
		Directory.SetCurrentDirectory(AppContext.BaseDirectory);

		IHost host = Host.CreateDefaultBuilder(args)
			.UseSerilog((context, loggerConfiguration) =>
			{
				loggerConfiguration.ReadFrom.Configuration(context.Configuration);
			})
			.ConfigureProcRoll()
			.UseWindowsService(options =>
			{
				options.ServiceName = "Burrow";
			})
			.Build();

		await host.RunAsync();
	}
}