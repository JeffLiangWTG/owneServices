using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using HtmlAgilityPack;

namespace Enterprise.EConversation.Business
{
	public static class EConversationUniqueIDUtil
	{
		public static string GenerateElement(BusinessObject parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			return $"<div name=\"{EConversationUniqueIDUtil.ElementName}\" id=\"{parent.TablePrefix}|{parent.PK}\">Please don't edit the content of this email as your message may not be processed</div>";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "HTML")]
		public static bool? TryLoadFromEmailBody<T>(BusinessObjectFactory factory, string emailBody, out T result) where T : BusinessObject
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			var doc = new HtmlDocument();
			doc.LoadHtml(emailBody);

			var idNode = doc.DocumentNode.Descendants("div").FirstOrDefault(n => n.Attributes["name"]?.Value.Contains(EConversationUniqueIDUtil.ElementName) == true);

			if (idNode == null)
			{
				result = null;

				return null;
			}

			var name = idNode.Attributes["name"].Value;
			var idx = name.IndexOf(EConversationUniqueIDUtil.ElementName);
			var prefix = name.Substring(0, idx);
			var suffix = name.Substring(idx + EConversationUniqueIDUtil.ElementName.Length);

			var id = idNode.Attributes["id"]?.Value;
			if (id != null)
			{
				if (id.StartsWith(prefix) && id.EndsWith(suffix))
				{
					id = id.Substring(prefix.Length, id.Length - prefix.Length - suffix.Length);
				}

				var data = id.Split('|');
				if (data?.Length == 2)
				{
					var code = new ZString(data[0]);
					var value = new ZString(data[1]);

					ZGuid pk;
					if (ZGuid.TryParse(value.Left(36), out pk) || ZGuid.TryParse(value.Left(32), out pk))
					{
						var tablePrefix = code.Right(2);
						result = factory.Load(tablePrefix, pk) as T;

						if (result == null)
						{
							tablePrefix = code.Right(3);
							result = factory.Load(tablePrefix, pk) as T;
						}

						return result != null;
					}
				}
			}

			result = null;

			return false;
		}

		public const string ElementName = "EConversationIdentifier";
	}
}
