using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.Security.Core.ResString;

namespace Enterprise.Security
{
	public class FormSecurityBuilder
	{
		internal readonly List<MultilingualString> TabSecurityPoints = new List<MultilingualString>();

		readonly SecurityCheckpoint root;
		readonly IZSecurity security;

		SecurityCheckpoint form;
		readonly List<SecurityCheckpoint> points = new List<SecurityCheckpoint>();

		public FormSecurityBuilder(SecurityCheckpoint root, IZSecurity security, bool hasViewSecurityPoint, bool hasEditSecurityPoint)
		{
			Argument.NotNull(root, "root");
			Argument.NotNull(security, "security");

			this.root = root;
			this.security = security;

			if (hasViewSecurityPoint)
			{
				TabSecurityPoints.Add(SecurityCore.Captions.View);
			}
			if (hasEditSecurityPoint)
			{
				TabSecurityPoints.Add(SecurityCore.Captions.Edit);
			}
		}

		public FormSecurityBuilder Form(string token)
		{
			Argument.NotNullOrEmpty(token, "token");

			form = new SecurityCheckpoint(token, ResString.GetMultilingualString("44278b3f-ff72-4998-b8e4-f601f27bfa67", "Screen"), root, security);
			foreach (var point in TabSecurityPoints)
			{
				points.Add(DeriveChild(form, point.GetUnresolvedString(), point, string.Empty));
			}

			return this;
		}

		public FormSecurityBuilder Tab(string token)
		{
			return Tab(token, (NoResString)string.Empty);
		}

		public FormSecurityBuilder Tab(string token, MultilingualString displayText)
		{
			return Tab(token, displayText, string.Empty);
		}

		public FormSecurityBuilder Tab(string token, MultilingualString displayText, string country)
		{
			Argument.NotNullOrEmpty(token, "token");

			if (string.IsNullOrEmpty(displayText))
			{
				if (token.EndsWith("TabPage", System.StringComparison.Ordinal))
				{
					displayText = (NoResString)token.Substring(0, token.Length - "TabPage".Length);
				}
				else
				{
					displayText = (NoResString)token;
				}
			}

			foreach (var point in points)
			{
				DeriveChild(point, token, displayText, country);
			}

			return this;
		}

		SecurityCheckpoint DeriveChild(SecurityCheckpoint parent, string childCode, MultilingualString displayText, string country)
		{
			return new SecurityCheckpoint(parent.Code + "." + childCode, displayText, parent, security, country);
		}
	}
}
