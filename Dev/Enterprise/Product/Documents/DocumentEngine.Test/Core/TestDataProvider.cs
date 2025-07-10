using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.Testing
{
	class TestDataProvider : IBODocDataProvider
	{
		public ZString Name
		{
			get { return fName; }
			set { fName = value; }
		}
		ZString fName;

		public ZString Phone
		{
			get { return fPhone; }
			set { fPhone = value; }
		}
		ZString fPhone;

		public ZInt IntField
		{
			get { return fIntField; }
			set { fIntField = value; }
		}
		ZInt fIntField;

		public ZDecimal DecimalField
		{
			get { return fDecimalField; }
			set { fDecimalField = value; }
		}
		ZDecimal fDecimalField;

		public ZDateTime DateTimeField
		{
			get { return fDateTimeField; }
			set { fDateTimeField = value; }
		}
		ZDateTime fDateTimeField;

		public ZDateTimeOffset DateTimeOffsetField
		{
			get { return fDateTimeOffsetField; }
			set { fDateTimeOffsetField = value; }
		}
		ZDateTimeOffset fDateTimeOffsetField;

		public ZBool BoolTrueField
		{
			get { return true; }
		}

		public ZBool BoolFalseField
		{
			get { return false; }
		}

		public TestDataProviderChild Child
		{
			get { return new TestDataProviderChild(); }
		}

		#region IBODocDataProvider Members
		//ZString IBODocDataProvider.DocTypeCode
		//{
		//    get
		//    {
		//        throw new Exception("The method or operation is not implemented.");
		//    }
		//    set
		//    {
		//        throw new Exception("The method or operation is not implemented.");
		//    }
		//}

		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		//void IBODocDataProvider.SetAdditionalCopyInfo(DocWrapperCopyInfo additionalCopyInfo)
		//{
		//    throw new Exception("The method or operation is not implemented.");
		//}

		BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		string[] IBODocDataProvider.ImageNamesToRemove
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		BusinessObject IBODocDataProvider.ParentBusinessObject
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		//ZString IFormatStringInterpreter.Format(ZString formatString)
		//{
		//    return new FormatStringInterpreter(this).Interpret(formatString);
		//}

		ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName)
		{
			return ZString.Empty;
		}

		string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName)
		{
			return string.Empty;
		}

		ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode)
		{
			return ZDateTime.Empty;
		}

		#endregion
	}
}
