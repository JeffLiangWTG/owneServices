using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Enterprise.xTMessaging.Shared;
using Xware.Xt.Grpc.Config;

namespace Enterprise.RemotePrinting.Client
{
	public sealed class NACCSUtils
	{
		const int HeaderLength = 398;

		public static IMsgClientProvider GetMsgClientProvider(IJPNACCSClientApplicationSetting setting)
		{
			return new BasicMsgClientProvider
			(
				setting.xTServerAddress,
				setting.xTServerCertificate,
				setting.xTApplicationNode,
				setting.xTPassword,
				TimeSpan.FromSeconds(setting.DirectxTMessagingConfig.XTIdleConnectionKeepAliveInSecondsValue),
				CancellationToken.None
			);
		}

		public static ISubmitMsgAttributeModifier GetSubmitMsgAttributeModifier(IJPNACCSClientApplicationSetting setting)
		{
			return new SubmitMsgAttributeModifier(setting);
		}

		public static string GetReceiver(IJPNACCSClientApplicationSetting setting)
		{
			return setting?.xTApplicationNode?.Split(new[] { "_" }, StringSplitOptions.None)?.FirstOrDefault() ?? string.Empty;
		}

		public static (string BusinessCode, string UserID, string MessageReference) ExtractOutboundHeaderInformation(byte[] message)
		{
			return
			(
				Encoding.ASCII.GetString(message.Skip(3).Take(5).ToArray()).Trim(),
				Encoding.ASCII.GetString(message.Skip(29).Take(8).ToArray()).Trim(),
				Encoding.ASCII.GetString(message.Skip(219).Take(26).ToArray()).Trim()
			);
		}

		public static (string BusinessCode, string UserCode, string MessageReference) ExtractInboundHeaderInformation(Stream message)
		{
			var header = new byte[HeaderLength];
			message.Read(header, 0, HeaderLength);

			return
			(
				Encoding.ASCII.GetString(header.Skip(3).Take(5).ToArray()).Trim(),
				Encoding.ASCII.GetString(header.Skip(29).Take(5).ToArray()).Trim(),
				Encoding.ASCII.GetString(header.Skip(219).Take(26).ToArray()).Trim()
			);
		}

		public static Configuration BuildxTConfiguration(IJPNACCSClientApplicationSetting setting)
		{
			var result = new Configuration();
			result.Connect = setting.xTServerAddress;
			result.CA = setting.xTServerCertificate;
			result.Application = new Application
			{
				URI = setting.xTApplicationNode,
				Password = setting.xTPassword
			};
			return result;
		}

		public static string ReadEmbeddedResource(string resourceName)
		{
			var targetAssembly = Assembly.GetCallingAssembly();
			var resourcePath = targetAssembly.GetManifestResourceNames().SingleOrDefault(c => c.EndsWith(resourceName));

			var resource = targetAssembly.GetManifestResourceStream(resourcePath) ?? throw new Exception($"Could not locate embedded resource '{resourcePath}'");

			resource.Seek(0, SeekOrigin.Begin);

			using var reader = new StreamReader(resource);
			return reader.ReadToEnd();
		}
	}
}
