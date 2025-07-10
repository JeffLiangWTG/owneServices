using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.FeatureControl;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Licensing.Billing.Business.Testing
{
	public static class FeatureControlTestHelper
	{
		public static void AssertGetFeatureData(ZDateTime utcNow, string featureCode, string parameterExpected)
		{
			using (ObjectCache.OverrideDateTimeProvider(new FeatureControlDateTimeProvider() { CurrentUtcDateTimeOverride = utcNow.ToDateTime() }))
			{
				var data = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(featureCode);
				if (parameterExpected == null)
				{
					Assertion.AssertNull(data);
				}
				else
				{
					Assertion.AssertEquals(parameterExpected, data.Parameter);
				}
			}
		}

		class FeatureControlDateTimeProvider : Environment.DateTimeProvider
		{
			public DateTime CurrentUtcDateTimeOverride { get; set; }
			public override DateTime CurrentUtcDateTime => CurrentUtcDateTimeOverride;
		}

		public static FeatureControlRule AddRule(string ruleCode, bool isGlobal, ZDate start, ZDate? end, string parameter)
		{
			var rule = new FeatureControlRule();
			rule.FCM_FeatureControlCode = ruleCode;
			rule.FCR_RuleType = isGlobal ? FeatureControlRuleFCR_RuleType.GLB : FeatureControlRuleFCR_RuleType.CLI;
			rule.FCR_StartDateUtc = start.ToDateTime();
			if (end.HasValue)
			{
				rule.FCR_EndDateUtc = end.Value.ToDateTime();
			}
			rule.FCR_Parameters = parameter;

			return rule;
		}

		public static string GetCompressedBase64String(FeatureControl featureControl)
		{
			using (var ms = new MemoryStream())
			{
				using (var zipStream = new GZipStream(ms, CompressionMode.Compress))
				{
					var xml = Serialize(featureControl);
					var xmlBytes = Encoding.UTF8.GetBytes(xml);
					zipStream.Write(xmlBytes, 0, xmlBytes.Length);
				}
				return Convert.ToBase64String(ms.ToArray());
			}
		}

		static string Serialize(FeatureControl featureControl)
		{
			using var stream = new MemoryStream();
			var xmlSerializer = new XmlSerializer(typeof(FeatureControl), string.Empty);
			xmlSerializer.Serialize(stream, featureControl);
			return StreamToString(stream);
		}

		static string StreamToString(Stream stream)
		{
			var position = stream.Position;
			if (stream.CanSeek)
			{
				stream.Position = 0;
			}

			using var streamReader = new StreamReader(stream, true);
			var result = streamReader.ReadToEnd();
			stream.Position = position;

			return result;
		}

		public static void SetRegistryValue(string value) =>
			WebDataRegistry.Instance.FeatureControlRuleContent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
	}
}
