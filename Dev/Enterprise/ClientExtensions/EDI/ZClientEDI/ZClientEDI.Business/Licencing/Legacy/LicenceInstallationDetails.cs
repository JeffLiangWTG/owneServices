using System.Xml;

namespace Enterprise.Client.EDI.Licencing.Business
{
	/// <summary>
	/// This is to support old header-level OnDemand licence type. New On Demand licences are defined at the line level.
	/// </summary>
	public interface IExposeDeprecatedOnDemandModeFlag
	{
		bool OnDemandMode { get; set; }
	}

	public class LicenceInstallationDetails : LicenceSegment, IExposeDeprecatedOnDemandModeFlag
	{
		public LicenceInstallationDetails()
		{
			this.AMSMode = "OFF";
		}

		public void Set(string aMSMode)
		{
			this.AMSMode = aMSMode;
		}

		public string AMSMode { get; private set; }

		#region Deprecated OnDemandMode flag

		bool OnDemandMode { get; set; }

		bool IExposeDeprecatedOnDemandModeFlag.OnDemandMode
		{
			get { return OnDemandMode; }
			set { OnDemandMode = value; }
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public void AddToLicenceNode(XmlNode licenceNode)
		{
			XmlDocument document = licenceNode.OwnerDocument;

			XmlAttribute aMSModeAttribute = document.CreateAttribute("AMSMode");
			aMSModeAttribute.Value = AMSMode;
			licenceNode.Attributes.Append(aMSModeAttribute);

			if (OnDemandMode)
			{
				XmlAttribute onDemandModeAttribute = document.CreateAttribute("OnDemand");
				onDemandModeAttribute.Value = OnDemandMode.ToString();
				licenceNode.Attributes.Append(onDemandModeAttribute);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public void LoadFromLicenceNode(XmlNode licenceNode)
		{
			this.AMSMode = GetStringAttributeValueSafely(licenceNode, "AMSMode", "OFF");
			this.OnDemandMode = GetBoolAttributeValueSafely(licenceNode, "OnDemand");
		}

		#endregion
	}
}

