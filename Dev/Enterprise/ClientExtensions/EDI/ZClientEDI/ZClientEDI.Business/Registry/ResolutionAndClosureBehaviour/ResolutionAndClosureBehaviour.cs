using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ResolutionAndClosureBehaviour : CodeDescriptionBoolTreeNode
	{
		public static class Constants
		{
			public static class Code
			{
				public const string AlwaysAllow = "ALW";
				public const string NeverAllow = "NEV";
			}

			public static class Description
			{
				public const string AlwaysAllow = "Always Allow";
				public const string NeverAllow = "Never Allow";
			}

			public static class CustomizedColumns
			{
				public const string AllowSelfResolve = "AllowSelfResolve";
				public const string DaysResolvedToClosed = "DaysResolvedToClosed";
				public const string DaysPendingCustomerToClosed = "DaysPendingCustomerToClosed";
				public const string ClosedReopenRule = "ClosedReopenRule";
			}
		}

		#region Schema

		protected new abstract class Schema : CodeDescriptionBoolTreeNode.Schema
		{
			public const string AllowSelfResolve = Constants.CustomizedColumns.AllowSelfResolve;
			public const string DaysResolvedToClosed = Constants.CustomizedColumns.DaysResolvedToClosed;
			public const string DaysPendingCustomerToClosed = Constants.CustomizedColumns.DaysPendingCustomerToClosed;
			public const string ClosedReopenRule = Constants.CustomizedColumns.ClosedReopenRule;
		}

		#endregion

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			DaysResolvedToClosed = 7;
			DaysPendingCustomerToClosed = 7;
			ClosedReopenRule = Constants.Code.AlwaysAllow;
		}

		#region Properties

		public ZBool AllowSelfResolve
		{
			get { return allowSelfResolve; }
			set
			{
				SetNonPersistentPropertyValue(AllowSelfResolveInfo, ref allowSelfResolve, value);
				if (!IsValidationSuspended)
				{
					ValidateAllowSelfResolve();
				}
			}
		}
		ZBool allowSelfResolve;

		public ZPropertyInfo AllowSelfResolveInfo
		{
			get { return GetZPropertyInfo(Schema.AllowSelfResolve); }
		}

		public ZInt DaysResolvedToClosed
		{
			get { return daysResolvedToClosed; }
			set
			{
				SetNonPersistentPropertyValue(DaysResolvedToClosedInfo, ref daysResolvedToClosed, value);
				if (!IsValidationSuspended)
				{
					ValidateDaysResolvedToClosed();
				}
			}
		}
		ZInt daysResolvedToClosed;

		public ZPropertyInfo DaysResolvedToClosedInfo
		{
			get { return GetZPropertyInfo(Schema.DaysResolvedToClosed); }
		}

		public ZInt DaysPendingCustomerToClosed
		{
			get { return daysPendingCustomerToClosed; }
			set
			{
				SetNonPersistentPropertyValue(DaysPendingCustomerToClosedInfo, ref daysPendingCustomerToClosed, value);
				if (!IsValidationSuspended)
				{
					ValidateDaysPendingCustomerToClosed();
				}
			}
		}
		ZInt daysPendingCustomerToClosed;

		public ZPropertyInfo DaysPendingCustomerToClosedInfo
		{
			get { return GetZPropertyInfo(Schema.DaysPendingCustomerToClosed); }
		}

		[List(nameof(ClosedReopenRuleList))]
		public ZString ClosedReopenRule
		{
			get { return closedReopenRule; }
			set
			{
				SetNonPersistentPropertyValue(ClosedReopenRuleInfo, ref closedReopenRule, value);
				if (!IsValidationSuspended)
				{
					ValidateClosedReopenRule();
				}
			}
		}
		ZString closedReopenRule;

		public ZPropertyInfo ClosedReopenRuleInfo
		{
			get { return GetZPropertyInfo(Schema.ClosedReopenRule); }
		}

		public CodeDescriptionPairList ClosedReopenRuleList
		{
			get
			{
				if (clsReopenRuleList == null)
				{
					clsReopenRuleList = new CodeDescriptionPairList();
					clsReopenRuleList.AddPair(Constants.Code.AlwaysAllow, Constants.Description.AlwaysAllow);
					clsReopenRuleList.AddPair(Constants.Code.NeverAllow, Constants.Description.NeverAllow);
				}
				return clsReopenRuleList;
			}
		}
		CodeDescriptionPairList clsReopenRuleList;

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			AllowSelfResolve = new ZBool(reader.ReadElementString(Schema.AllowSelfResolve));
			DaysResolvedToClosed = new ZInt(reader.ReadElementString(Schema.DaysResolvedToClosed));
			DaysPendingCustomerToClosed = new ZInt(reader.ReadElementString(Schema.DaysPendingCustomerToClosed));
			ClosedReopenRule = new ZString(reader.ReadElementString(Schema.ClosedReopenRule));

			if (ParentID.IsEmpty)
			{
				SystemDefined = true;
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.AllowSelfResolve, AllowSelfResolve.ToString());
			writer.WriteElementString(Schema.DaysResolvedToClosed, DaysResolvedToClosed.ToString());
			writer.WriteElementString(Schema.DaysPendingCustomerToClosed, DaysPendingCustomerToClosed.ToString());
			writer.WriteElementString(Schema.ClosedReopenRule, ClosedReopenRule);
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ResolutionAndClosureBehaviour();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var node = (ResolutionAndClosureBehaviour)clone;
			node.AllowSelfResolve = AllowSelfResolve;
			node.DaysResolvedToClosed = DaysResolvedToClosed;
			node.DaysPendingCustomerToClosed = DaysPendingCustomerToClosed;
			node.ClosedReopenRule = ClosedReopenRule;
		}

		#endregion

		#region Validation

		public void ValidateAllowSelfResolve()
		{
		}

		public void ValidateDaysResolvedToClosed()
		{
			DaysResolvedToClosedInfo.ClearAllNotifications();

			if (DaysResolvedToClosed < 1 || DaysResolvedToClosed >= 99)
			{
				DaysResolvedToClosedInfo.AddError("Resolved to Closed days value must be between 1 and 98.");
			}
		}

		public void ValidateDaysPendingCustomerToClosed()
		{
			DaysPendingCustomerToClosedInfo.ClearAllNotifications();

			if (DaysPendingCustomerToClosed < 1 || DaysPendingCustomerToClosed >= 99)
			{
				DaysPendingCustomerToClosedInfo.AddError("Pending to Closed days value must be between 1 and 98.");
			}
		}

		public void ValidateClosedReopenRule()
		{
			ClosedReopenRuleInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ClosedReopenRuleInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAllowSelfResolve();
			ValidateDaysResolvedToClosed();
			ValidateDaysPendingCustomerToClosed();
			ValidateClosedReopenRule();
		}

		#endregion
	}
}

