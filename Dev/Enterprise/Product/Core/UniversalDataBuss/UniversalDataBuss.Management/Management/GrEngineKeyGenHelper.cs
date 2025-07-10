using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Management.SessionLogging;

namespace Enterprise.UniversalDataBuss.Management
{
	public static class GrEngineKeyGenHelper
	{
		public const string FileNamePrefix = "KeyInfo";

		public static void GenerateKey(IEDIMessage message, string directory)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message), "Message can not be null");
			}
			if (string.IsNullOrEmpty(directory))
			{
				throw new ArgumentNullException(nameof(directory), "Directory can not be empty");
			}

			var logger = new TaskLogger(new SimpleLogger());
			using (ObjectFactory.Get<ISuppressHookHelper>().SuppressFieldOnChangeHook())
			{
				var stopwatch = Stopwatch.StartNew();
				var provider = (IMessageKeyProvider)UniversalMessageProcessingExtensions.GetMessageProcessor(message.EM_MessageSubType);
				logger.Log(LogType.Information, $"Provider type: {provider.GetType().Name}");
				var result = provider.GetKeysForBlockingParallelImport(new UniversalObjectFactory(), message, new XmlSessionTracker(logger));
				stopwatch.Stop();
				var fileName = $"{FileNamePrefix}{(string.IsNullOrEmpty(message.EM_MessageNum) ? "" : "_" + message.EM_MessageNum)}.txt";
				WriteKeysToFile(result.KeysInfo, result.Keys, Path.Combine(directory, fileName), string.Join("\r\n ", logger.Logs.Select(l => l.Message)), stopwatch.ElapsedMilliseconds);
			}
		}

		static void WriteKeysToFile(IEnumerable<(string KeyValue, string KeySource)> keysInfo, IEnumerable<string> keys, string filePath, string log, long elapsed)
		{
			using (var writer = new StreamWriter(filePath))
			{
				if (keys == null || keysInfo == null)
				{
					writer.WriteLine("No keys generated");
					return;
				}
				writer.WriteLine("=== Key Summary ===");
				writer.WriteLine($"Elapsed time: {elapsed} ms");
				writer.WriteLine($"Total Keys: {keys.Count()}");
				writer.WriteLine(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"Key Values: {string.Join(", ", keys)}")));

				writer.WriteLine("\n=== Key Details ===");

				foreach (var keyInfo in keysInfo)
				{
					var hexValue = BitConverter.ToString(Encoding.UTF8.GetBytes(keyInfo.KeyValue)).Replace("-", " ");
					writer.WriteLine(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"  Key Value: {keyInfo.KeyValue}")));
					writer.WriteLine($"  Source: {keyInfo.KeySource}");
					writer.WriteLine($"  Hex Bytes: {hexValue}");
				}

				writer.WriteLine("\n=== Logs ===");
				writer.WriteLine(log);
			}
		}
	}
}
