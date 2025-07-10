using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class PKDescription : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PKDescription()
		{
			this.pk = ZGuid.NewZGuid();
		}

		public PKDescription(ZGuid pk, ZString description)
		{
			this.pk = pk;
			this.description = description;
		}

		#region Schema

		public abstract class Schema
		{
			public const string Description = "Description";
		}

		#endregion

		#region PK

		protected override ZGuid GetPK()
		{
			return pk;
		}
		readonly ZGuid pk;

		#endregion

		#region Description

		[CargoWise.ComponentModel.MaxLength(500)]
		public ZString Description
		{
			get { return description; }
			set
			{
				if (description != value)
				{
					CheckMaximumLength(DescriptionInfo, value);
					description = value;
				}
				DescriptionInfo.RefreshBinding();
			}
		}
		ZString description;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#region DoNotCreateHyperLink

		public bool DoNotCreateHyperLink
		{
			get { return doNotCreateHyperLink; }
			set { doNotCreateHyperLink = value; }
		}
		bool doNotCreateHyperLink;

		#endregion
	}
}
