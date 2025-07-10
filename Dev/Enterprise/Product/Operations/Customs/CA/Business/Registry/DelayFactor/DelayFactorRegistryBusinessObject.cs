using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.CA.Business.XmlSerializers")]
	public class DelayFactorRegistryBusinessObject : RegistryBusinessObjectTemplate
	{
		public DelayFactorRegistryBusinessObject()
		{
		}

		public DelayFactorRegistryBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DelayFactorRegistryBusinessObject(ZInt hvsDelayInterval, ZString hvsDelayIntervalType, ZInt conDelayInterval, ZString conDelayIntervalType)
		{
			HVSDelayInterval = hvsDelayInterval;
			HVSDelayIntervalType = hvsDelayIntervalType;
			CONDelayInterval = conDelayInterval;
			CONDelayIntervalType = conDelayIntervalType;
		}

		#region Schema & Constants

		public abstract class Schema
		{
			public const string HVSDelayInterval = "HVSDelayInterval";
			public const string HVSDelayIntervalType = "HVSDelayIntervalType";
			public const string CONDelayInterval = "CONDelayInterval";
			public const string CONDelayIntervalType = "CONDelayIntervalType";
		}

		#endregion

		#region HVSDelayInterval

		[ReadOnlyMember(nameof(HVSDelayInterval_ReadOnly))]
		public ZInt HVSDelayInterval
		{
			get { return hvsDelayInterval; }
			set
			{
				SetNonPersistentPropertyValue(HVSDelayIntervalInfo, ref hvsDelayInterval, value);
				ValidateHVSDelayInterval();
			}
		}
		ZInt hvsDelayInterval = 0;

		ZBool HVSDelayInterval_ReadOnly
		{
			get { return HVSDelayIntervalType == DelayIntervalTypeCodes.Codes.None; }
		}

		public ZPropertyInfo HVSDelayIntervalInfo
		{
			get { return GetZPropertyInfo(Schema.HVSDelayInterval); }
		}

		public void ValidateHVSDelayInterval()
		{
			HVSDelayIntervalInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(HVSDelayIntervalInfo);
		}

		#endregion

		#region HVSDelayIntervalType

		[MaxLength(3)]
		[List(nameof(HVSDelayIntervalTypeList))]
		public ZString HVSDelayIntervalType
		{
			get { return hvsDelayIntervalType; }
			set
			{
				CheckMaximumLength(HVSDelayIntervalTypeInfo, value);
				SetNonPersistentPropertyValue(HVSDelayIntervalTypeInfo, ref hvsDelayIntervalType, value);
				ValidateHVSDelayIntervalType();
			}
		}
		ZString hvsDelayIntervalType = DelayIntervalTypeCodes.Codes.None;

		public ZPropertyInfo HVSDelayIntervalTypeInfo
		{
			get { return GetZPropertyInfo(Schema.HVSDelayIntervalType); }
		}

		public void ValidateHVSDelayIntervalType()
		{
			HVSDelayIntervalTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(HVSDelayIntervalTypeInfo);
			ListValidation.ErrorIfInvalidCode(HVSDelayIntervalTypeInfo, HVSDelayIntervalTypeList);
		}

		public CodeDescriptionPairList HVSDelayIntervalTypeList
		{
			get
			{
				return GetFilteredDelayIntervalTypeList(HVSCodesToExclude);
			}
		}

		CodeDescriptionPairList GetFilteredDelayIntervalTypeList(string codesToExclude)
		{
			var result = new DelayIntervalTypeCodes();
			foreach (string code in codesToExclude.Split(','))
			{
				if (result.ContainsCode(code))
				{
					result.RemoveCode(code);
				}
			}
			return result;
		}

		#endregion

		#region HVSCodesToExclude

		[MaxLength(15)]
		public ZString HVSCodesToExclude => new ZStringBuilder(new[] { DelayIntervalTypeCodes.Codes.Default, DelayIntervalTypeCodes.Codes.DAS, DelayIntervalTypeCodes.Codes.DAY }).ToStringWithDelimiterBetweenAppends(",");

		#endregion

		#region CONDelayInterval

		[ReadOnlyMember(nameof(CONDelayInterval_ReadOnly))]
		public ZInt CONDelayInterval
		{
			get { return conDelayInterval; }
			set
			{
				SetNonPersistentPropertyValue(CONDelayIntervalInfo, ref conDelayInterval, value);
				ValidateCONDelayInterval();
			}
		}
		ZInt conDelayInterval = 0;

		ZBool CONDelayInterval_ReadOnly
		{
			get { return CONDelayIntervalType == DelayIntervalTypeCodes.Codes.None; }
		}

		public ZPropertyInfo CONDelayIntervalInfo
		{
			get { return GetZPropertyInfo(Schema.CONDelayInterval); }
		}

		public void ValidateCONDelayInterval()
		{
			CONDelayIntervalInfo.ClearAllNotifications();
			if (CONDelayIntervalType == DelayIntervalTypeCodes.Codes.DAY)
			{
				MandatoryValidation.CheckNotZero(CONDelayIntervalInfo);
				MandatoryValidation.CheckNotNegative(CONDelayIntervalInfo);
				if ((ZInt)CONDelayIntervalInfo.Value >= 25)
				{
					CONDelayIntervalInfo.AddError(Business.Res.GetString("58EAB95B-1FF0-4C90-AD1F-8969D29D7BEC", "Goods must be accounted for by the 24th of the month."));
				}
			}
		}

		#endregion

		#region CONDelayIntervalType

		[MaxLength(3)]
		[List(nameof(CONDelayIntervalTypeList))]
		public ZString CONDelayIntervalType
		{
			get { return conDelayIntervalType; }
			set
			{
				CheckMaximumLength(CONDelayIntervalTypeInfo, value);
				SetNonPersistentPropertyValue(CONDelayIntervalTypeInfo, ref conDelayIntervalType, value);
				ValidateCONDelayIntervalType();
				ValidateCONDelayInterval();
			}
		}
		ZString conDelayIntervalType = DelayIntervalTypeCodes.Codes.None;

		public ZPropertyInfo CONDelayIntervalTypeInfo
		{
			get { return GetZPropertyInfo(Schema.CONDelayIntervalType); }
		}

		public void ValidateCONDelayIntervalType()
		{
			CONDelayIntervalTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CONDelayIntervalTypeInfo);
			ListValidation.ErrorIfInvalidCode(CONDelayIntervalTypeInfo, CONDelayIntervalTypeList);
		}

		public CodeDescriptionPairList CONDelayIntervalTypeList
		{
			get
			{
				return GetFilteredDelayIntervalTypeList(CONCodesToExclude);
			}
		}

		#endregion

		#region CONCodesToExclude

		[MaxLength(15)]
		public ZString CONCodesToExclude
			=>
				new ZStringBuilder(new[]
				{ DelayIntervalTypeCodes.Codes.Default, DelayIntervalTypeCodes.Codes.DAR, DelayIntervalTypeCodes.Codes.DAS })
					.ToStringWithDelimiterBetweenAppends(",");

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.HVSDelayInterval, HVSDelayInterval.ToString());
			writer.WriteElementString(Schema.HVSDelayIntervalType, HVSDelayIntervalType.ToString());
			writer.WriteElementString(Schema.CONDelayInterval, CONDelayInterval.ToString());
			writer.WriteElementString(Schema.CONDelayIntervalType, CONDelayIntervalType.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			hvsDelayInterval = ZInt.Parse(reader.ReadElementString(Schema.HVSDelayInterval));
			hvsDelayIntervalType = new ZString(reader.ReadElementString(Schema.HVSDelayIntervalType));
			reader.ReadElementString("LVSDelayInterval");
			reader.ReadElementString("LVSDelayIntervalType");
			conDelayInterval = ZInt.Parse(reader.ReadElementString(Schema.CONDelayInterval));
			conDelayIntervalType = new ZString(reader.ReadElementString(Schema.CONDelayIntervalType));
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DelayFactorRegistryBusinessObject(factory);
		}

		public override bool Equals(object obj)
		{
			var compareObj = obj as DelayFactorRegistryBusinessObject;
			return compareObj == null ? base.Equals(obj)
					: this.HVSDelayInterval == compareObj.HVSDelayInterval &&
					this.HVSDelayIntervalType == compareObj.HVSDelayIntervalType &&
					this.CONDelayInterval == compareObj.CONDelayInterval &&
					this.CONDelayIntervalType == compareObj.CONDelayIntervalType;
		}

		public override int GetHashCode()
		{
			return HVSDelayInterval.GetHashCode() ^ HVSDelayIntervalType.GetHashCode() ^
						CONDelayInterval.GetHashCode() ^ CONDelayIntervalType.GetHashCode();
		}
	}
}
