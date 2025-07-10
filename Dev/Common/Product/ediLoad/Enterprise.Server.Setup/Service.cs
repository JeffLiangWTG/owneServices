namespace Enterprise.Server.Setup
{
	using System;
	using System.Collections;
	using System.ComponentModel;
	using System.Management;
	using CargoWise.Common;

	// Functions ShouldSerialize<PropertyName> are functions used by VS property browser to check if a particular property has to be serialized. These functions are added for all ValueType properties ( properties of type Int32, BOOL etc.. which cannot be set to null). These functions use Is<PropertyName>Null function. These functions are also used in the TypeConverter implementation for the properties to check for NULL value of property so that an empty value can be shown in Property browser in case of Drag and Drop in Visual studio.
	// Functions Is<PropertyName>Null() are used to check if a property is NULL.
	// Functions Reset<PropertyName> are added for Nullable Read/Write properties. These functions are used by VS designer in property browser to set a property to NULL.
	// Every property added to the class for WMI property has attributes set to define its behavior in Visual Studio designer and also to define a TypeConverter to be used.
	// Datetime conversion functions ToDateTime and ToDmtfDateTime are added to the class to convert DMTF datetime to System.DateTime and vice-versa.
	// An Early Bound class generated for the WMI class.Win32_Service
	sealed class Service : Component
	{
		// Private property to hold the WMI namespace in which the class resides.
		static readonly string CreatedWmiNamespace = "root\\CimV2";

		// Private property to hold the name of WMI class which created this class.
		static readonly string CreatedClassName = "Win32_Service";

		// Private member variable to hold the ManagementScope which is used by the various methods.
		[ThreadStatic]
		static ManagementScope statMgmtScope;

		ManagementSystemProperties PrivateSystemProperties;

		// Underlying lateBound WMI object.
		ManagementObject PrivateLateBoundObject;

		// Member variable to store the 'automatic commit' behavior for the class.
		bool AutoCommitProp;

		// Private variable to hold the embedded property representing the instance.
		readonly ManagementBaseObject embeddedObj;

		// The current WMI object used
		ManagementBaseObject curObj;

		// Flag to indicate if the instance is an embedded object.
		bool isEmbedded;

		// Below are different overloads of constructors to initialize an instance of the class with a WMI object.
		public Service()
		{
			this.InitializeObject(null, null, null);
		}

		public Service(string keyName)
		{
			this.InitializeObject(null, new ManagementPath(ConstructPath(keyName)), null);
		}

		public Service(ManagementScope mgmtScope, string keyName)
		{
			this.InitializeObject(mgmtScope, new ManagementPath(ConstructPath(keyName)), null);
		}

		public Service(ManagementPath path, ObjectGetOptions getOptions)
		{
			this.InitializeObject(null, path, getOptions);
		}

		public Service(ManagementScope mgmtScope, ManagementPath path)
		{
			this.InitializeObject(mgmtScope, path, null);
		}

		public Service(ManagementPath path)
		{
			this.InitializeObject(null, path, null);
		}

		public Service(ManagementScope mgmtScope, ManagementPath path, ObjectGetOptions getOptions)
		{
			this.InitializeObject(mgmtScope, path, getOptions);
		}

		public Service(ManagementObject theObject)
		{
			Initialize();
			if ((CheckIfProperClass(theObject)))
			{
				PrivateLateBoundObject = theObject;
				PrivateSystemProperties = new ManagementSystemProperties(PrivateLateBoundObject);
				curObj = PrivateLateBoundObject;
			}
			else
			{
				throw new ArgumentException("Class name does not match.");
			}
		}

		public Service(ManagementBaseObject theObject)
		{
			Initialize();
			if ((CheckIfProperClass(theObject)))
			{
				embeddedObj = theObject;
				PrivateSystemProperties = new ManagementSystemProperties(theObject);
				curObj = embeddedObj;
				isEmbedded = true;
			}
			else
			{
				throw new ArgumentException("Class name does not match.");
			}
		}

		// Property returns the namespace of the WMI class.
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string OriginatingNamespace
		{
			get
			{
				return "root\\CimV2";
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ManagementClassName
		{
			get
			{
				string strRet = CreatedClassName;
				if ((curObj != null))
				{
					if ((curObj.ClassPath != null))
					{
						strRet = ((string)(curObj["__CLASS"]));
						if (((strRet == null)
									|| (string.IsNullOrEmpty(strRet))))
						{
							strRet = CreatedClassName;
						}
					}
				}
				return strRet;
			}
		}

		// Property pointing to an embedded object to get System properties of the WMI object.
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ManagementSystemProperties SystemProperties
		{
			get
			{
				return PrivateSystemProperties;
			}
		}

		// Property returning the underlying lateBound object.
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ManagementBaseObject LateBoundObject
		{
			get
			{
				return curObj;
			}
		}

		// ManagementScope of the object.
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ManagementScope Scope
		{
			get
			{
				if ((!isEmbedded))
				{
					return PrivateLateBoundObject.Scope;
				}
				else
				{
					return null;
				}
			}
			set
			{
				if ((!isEmbedded))
				{
					PrivateLateBoundObject.Scope = value;
				}
			}
		}

		// Property to show the commit behavior for the WMI object. If true, WMI object will be automatically saved after each property modification.(ie. Put() is called after modification of a property).
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AutoCommit
		{
			get
			{
				return AutoCommitProp;
			}
			set
			{
				AutoCommitProp = value;
			}
		}

		// The ManagementPath of the underlying WMI object.
		[Browsable(true)]
		public ManagementPath Path
		{
			get
			{
				if ((!isEmbedded))
				{
					return PrivateLateBoundObject.Path;
				}
				else
				{
					return null;
				}
			}
			set
			{
				if ((!isEmbedded))
				{
					if ((!CheckIfProperClass(null, value, null)))
					{
						throw new ArgumentException("Class name does not match.");
					}
					PrivateLateBoundObject.Path = value;
				}
			}
		}

		// Public static scope property which is used by the various methods.
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public static ManagementScope StaticScope
		{
			get
			{
				return statMgmtScope;
			}
			set
			{
				statMgmtScope = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsAcceptPauseNull
		{
			get
			{
				if ((curObj[nameof(AcceptPause)] == null))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The AcceptPause property indicates whether the service can be paused.\nValues: TRU" +
			"E or FALSE. A value of TRUE indicates the service can be paused.")]
		[TypeConverter(typeof(WMIValueTypeConverter))]
		public bool AcceptPause
		{
			get
			{
				if ((curObj[nameof(AcceptPause)] == null))
				{
					return Convert.ToBoolean(0);
				}
				return ((bool)(curObj[nameof(AcceptPause)]));
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsAcceptStopNull
		{
			get
			{
				if ((curObj[nameof(AcceptStop)] == null))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The AcceptStop property indicates whether the service can be stopped.\nValues: TRU" +
			"E or FALSE. A value of TRUE indicates the service can be stopped.")]
		[TypeConverter(typeof(WMIValueTypeConverter))]
		public bool AcceptStop
		{
			get
			{
				if ((curObj[nameof(AcceptStop)] == null))
				{
					return Convert.ToBoolean(0);
				}
				return ((bool)(curObj[nameof(AcceptStop)]));
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The Caption property is a short textual description (one-line string) of the obje" +
			"ct.")]
		public string Caption
		{
			get
			{
				return ((string)(curObj[nameof(Caption)]));
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsCheckPointNull
		{
			get
			{
				if ((curObj[nameof(CheckPoint)] == null))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description(@"The CheckPoint property specifies a value that the service increments periodically to report its progress during a lengthy start, stop, pause, or continue operation. For example, the service should increment this value as it completes each step of its initialization when it is starting up. The user interface program that invoked the operation on the service uses this value to track the progress of the service during a lengthy operation. This value is not valid and should be zero when the service does not have a start, stop, pause, or continue operation pending.")]
		[TypeConverter(typeof(WMIValueTypeConverter))]
		public uint CheckPoint
		{
			get
			{
				if ((curObj[nameof(CheckPoint)] == null))
				{
					return Convert.ToUInt32(0);
				}
				return ((uint)(curObj[nameof(CheckPoint)]));
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("CreationClassName indicates the name of the class or the subclass used in the cre" +
			"ation of an instance. When used with the other key properties of this class, thi" +
			"s property allows all instances of this class and its subclasses to be uniquely " +
			"identified.")]
		public string CreationClassName
		{
			get
			{
				return ((string)(curObj[nameof(CreationClassName)]));
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The Description property provides a textual description of the object. ")]
		public string Description
		{
			get
			{
				return ((string)(curObj[nameof(Description)]));
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsDesktopInteractNull
		{
			get
			{
				if ((curObj[nameof(DesktopInteract)] == null))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The DesktopInteract property indicates whether the service can create or communic" +
			"ate with windows on the desktop.\nValues: TRUE or FALSE. A value of TRUE indicate" +
			"s the service can create or communicate with windows on the desktop.")]
		[TypeConverter(typeof(WMIValueTypeConverter))]
		public bool DesktopInteract
		{
			get
			{
				if ((curObj[nameof(DesktopInteract)] == null))
				{
					return Convert.ToBoolean(0);
				}
				return ((bool)(curObj[nameof(DesktopInteract)]));
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description(@"The DisplayName property indicates the display name of the service. This string has a maximum length of 256 characters. The name is case-preserved in the Service Control Manager. DisplayName comparisons are always case-insensitive. 
Constraints: Accepts the same value as the Name property.
Example: Atdisk.")]
		public string DisplayName
		{
			get
			{
				return ((string)(curObj[nameof(DisplayName)]));
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description(@"If this service fails to start during startup, the ErrorControl property specifies the severity of the error. The value indicates the action taken by the startup program if failure occurs. All errors are logged by the computer system. The computer system does not notify the user of ""Ignore"" errors. With ""Normal"" errors the user is notified. With ""Severe"" errors, the system is restarted with the last-known-good configuration. Finally, on""Critical"" errors the system attempts to restart with a good configuration.")]
		public string ErrorControl
		{
			get
			{
				return ((string)(curObj[nameof(ErrorControl)]));
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsExitCodeNull
		{
			get
			{
				if ((curObj[nameof(ExitCode)] == null))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description(@"The ExitCode property specifies a Win32 error code defining any problems encountered in starting or stopping the service. This property is set to ERROR_SERVICE_SPECIFIC_ERROR (1066) when the error is unique to the service represented by this class, and information about the error is available in the ServiceSpecificExitCode member. The service sets this value to NO_ERROR when running, and again upon normal termination.")]
		[TypeConverter(typeof(WMIValueTypeConverter))]
		public uint ExitCode
		{
			get
			{
				if ((curObj[nameof(ExitCode)] == null))
				{
					return Convert.ToUInt32(0);
				}
				return ((uint)(curObj[nameof(ExitCode)]));
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsInstallDateNull
		{
			get
			{
				if ((curObj[nameof(InstallDate)] == null))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The InstallDate property is datetime value indicating when the object was install" +
			"ed. A lack of a value does not indicate that the object is not installed.")]
		[TypeConverter(typeof(WMIValueTypeConverter))]
		public DateTime InstallDate
		{
			get
			{
				if ((curObj[nameof(InstallDate)] != null))
				{
					return ToDateTime(((string)(curObj[nameof(InstallDate)])));
				}
				else
				{
					return DateTime.MinValue;
				}
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The Name property uniquely identifies the service and provides an indication of t" +
			"he functionality that is managed. This functionality is described in more detail" +
			" in the object\'s Description property. ")]
		public string Name
		{
			get
			{
				return ((string)(curObj[nameof(Name)]));
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The PathName property contains the fully qualified path to the service binary fil" +
			"e that implements the service.\nExample: \\SystemRoot\\System32\\drivers\\afd.sys")]
		public string PathName
		{
			get
			{
				return ((string)(curObj[nameof(PathName)]));
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsProcessIdNull
		{
			get
			{
				if ((curObj[nameof(ProcessId)] == null))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The ProcessId property specifies the process identifier of the service.\nExample: " +
			"324")]
		[TypeConverter(typeof(WMIValueTypeConverter))]
		public uint ProcessId
		{
			get
			{
				if ((curObj[nameof(ProcessId)] == null))
				{
					return Convert.ToUInt32(0);
				}
				return ((uint)(curObj[nameof(ProcessId)]));
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsServiceSpecificExitCodeNull
		{
			get
			{
				if ((curObj[nameof(ServiceSpecificExitCode)] == null))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description(@"The ServiceSpecificExitCode property specifies a service-specific error code for errors that occur while the service is either starting or stopping. The exit codes are defined by the service represented by this class. This value is only set when the ExitCodeproperty value is ERROR_SERVICE_SPECIFIC_ERROR, 1066.")]
		[TypeConverter(typeof(WMIValueTypeConverter))]
		public uint ServiceSpecificExitCode
		{
			get
			{
				if ((curObj[nameof(ServiceSpecificExitCode)] == null))
				{
					return Convert.ToUInt32(0);
				}
				return ((uint)(curObj[nameof(ServiceSpecificExitCode)]));
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The ServiceType property supplies the type of service provided to calling process" +
			"es.")]
		public string ServiceType
		{
			get
			{
				return ((string)(curObj[nameof(ServiceType)]));
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsStartedNull
		{
			get
			{
				if ((curObj[nameof(Started)] == null))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("Started is a boolean indicating whether the service has been started (TRUE), or s" +
			"topped (FALSE).")]
		[TypeConverter(typeof(WMIValueTypeConverter))]
		public bool Started
		{
			get
			{
				if ((curObj[nameof(Started)] == null))
				{
					return Convert.ToBoolean(0);
				}
				return ((bool)(curObj[nameof(Started)]));
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description(@"The StartMode property indicates the start mode of the Win32 base service. ""Boot"" specifies a device driver started by the operating system loader. This value is valid only for driver services. ""System"" specifies a device driver started by the IoInitSystem function. This value is valid only for driver services. ""Automatic"" specifies a service to be started automatically by the service control manager during system startup. ""Manual"" specifies a service to be started by the service control manager when a process calls the StartService function. ""Disabled"" specifies a service that can no longer be started.")]
		public string StartMode
		{
			get
			{
				return ((string)(curObj[nameof(StartMode)]));
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description(@"The StartName property indicates the account name under which the service runs. Depending on the service type, the account name may be in the form of ""DomainName\Username"".  The service process will be logged using one of these two forms when it runs. If the account belongs to the built-in domain, "".\Username"" can be specified. If NULL is specified, the service will be logged on as the LocalSystem account. For kernel or system level drivers, StartName contains the driver object name (that is, \FileSystem\Rdr or \Driver\Xns) which the input and output (I/O) system uses to load the device driver. Additionally, if NULL is specified, the driver runs with a default object name created by the I/O system based on the service name.
Example: DWDOM\Admin.")]
		public string StartName
		{
			get
			{
				return ((string)(curObj[nameof(StartName)]));
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The State property indicates the current state of the base service.")]
		public string State
		{
			get
			{
				return ((string)(curObj[nameof(State)]));
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description(@"The Status property is a string indicating the current status of the object. Various operational and non-operational statuses can be defined. Operational statuses are ""OK"", ""Degraded"" and ""Pred Fail"". ""Pred Fail"" indicates that an element may be functioning properly but predicting a failure in the near future. An example is a SMART-enabled hard drive. Non-operational statuses can also be specified. These are ""Error"", ""Starting"", ""Stopping"" and ""Service"". The latter, ""Service"", could apply during mirror-resilvering of a disk, reload of a user permissions list, or other administrative work. Not all such work is on-line, yet the managed element is neither ""OK"" nor in one of the other states.")]
		public string Status
		{
			get
			{
				return ((string)(curObj[nameof(Status)]));
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The scoping System\'s CreationClassName. ")]
		public string SystemCreationClassName
		{
			get
			{
				return ((string)(curObj[nameof(SystemCreationClassName)]));
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The name of the system that hosts this service")]
		public string SystemName
		{
			get
			{
				return ((string)(curObj[nameof(SystemName)]));
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsTagIdNull
		{
			get
			{
				if ((curObj[nameof(TagId)] == null))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description(@"The TagId property specifies a unique tag value for this service in the group. A value of 0 indicates that the service has not been assigned a tag. A tag can be used for ordering service startup within a load order group by specifying a tag order vector in the registry located at: HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\GroupOrderList. Tags are only evaluated for Kernel Driver and File System Driver start type services that have ""Boot"" or ""System"" start modes.")]
		[TypeConverter(typeof(WMIValueTypeConverter))]
		public uint TagId
		{
			get
			{
				if ((curObj[nameof(TagId)] == null))
				{
					return Convert.ToUInt32(0);
				}
				return ((uint)(curObj[nameof(TagId)]));
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsWaitHintNull
		{
			get
			{
				if ((curObj[nameof(WaitHint)] == null))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description(@"The WaitHint property specifies the estimated time required (in milliseconds) for a pending start, stop, pause, or continue operation. After the specified amount of time has elapsed, the service makes its next call to the SetServiceStatus function with either an incremented CheckPoint value or a change in Current State. If the amount of time specified by WaitHint passes, and CheckPoint has not been incremented, or the Current State has not changed, the service control manager or service control program assumes that an error has occurred.")]
		[TypeConverter(typeof(WMIValueTypeConverter))]
		public uint WaitHint
		{
			get
			{
				if ((curObj[nameof(WaitHint)] == null))
				{
					return Convert.ToUInt32(0);
				}
				return ((uint)(curObj[nameof(WaitHint)]));
			}
		}

		bool CheckIfProperClass(ManagementScope mgmtScope, ManagementPath path, ObjectGetOptions optionsParam)
		{
			if (((path != null)
						&& (string.Compare(path.ClassName, this.ManagementClassName, true, System.Globalization.CultureInfo.InvariantCulture) == 0)))
			{
				return true;
			}
			else
			{
				return CheckIfProperClass(new ManagementObject(mgmtScope, path, optionsParam));
			}
		}

		bool CheckIfProperClass(ManagementBaseObject theObj)
		{
			if (((theObj != null)
						&& (string.Compare(((string)(theObj["__CLASS"])), this.ManagementClassName, true, System.Globalization.CultureInfo.InvariantCulture) == 0)))
			{
				return true;
			}
			else
			{
				Array parentClasses = ((Array)(theObj["__DERIVATION"]));
				if ((parentClasses != null))
				{
					int count = 0;
					for (count = 0; (count < parentClasses.Length); count = (count + 1))
					{
						if ((string.Compare(((string)(parentClasses.GetValue(count))), this.ManagementClassName, true, System.Globalization.CultureInfo.InvariantCulture) == 0))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		bool ShouldSerializeAcceptPause()
		{
			if ((!this.IsAcceptPauseNull))
			{
				return true;
			}
			return false;
		}

		bool ShouldSerializeAcceptStop()
		{
			if ((!this.IsAcceptStopNull))
			{
				return true;
			}
			return false;
		}

		bool ShouldSerializeCheckPoint()
		{
			if ((!this.IsCheckPointNull))
			{
				return true;
			}
			return false;
		}

		bool ShouldSerializeDesktopInteract()
		{
			if ((!this.IsDesktopInteractNull))
			{
				return true;
			}
			return false;
		}

		bool ShouldSerializeExitCode()
		{
			if ((!this.IsExitCodeNull))
			{
				return true;
			}
			return false;
		}

		// Converts a given datetime in DMTF format to System.DateTime object.
		static DateTime ToDateTime(string dmtfDate)
		{
			DateTime initializer = DateTime.MinValue;
			int year = initializer.Year;
			int month = initializer.Month;
			int day = initializer.Day;
			int hour = initializer.Hour;
			int minute = initializer.Minute;
			int second = initializer.Second;
			long ticks = 0;
			string dmtf = dmtfDate;
			DateTime datetime = DateTime.MinValue;
			string tempString = string.Empty;
			if ((dmtf == null))
			{
				throw new ArgumentOutOfRangeException(nameof(dmtfDate));
			}
			if ((dmtf.Length == 0))
			{
				throw new ArgumentOutOfRangeException(nameof(dmtfDate));
			}
			if ((dmtf.Length != 25))
			{
				throw new ArgumentOutOfRangeException(nameof(dmtfDate));
			}
			try
			{
				tempString = dmtf.Substring(0, 4);
				if (("****" != tempString))
				{
					year = int.Parse(tempString);
				}
				tempString = dmtf.Substring(4, 2);
				if (("**" != tempString))
				{
					month = int.Parse(tempString);
				}
				tempString = dmtf.Substring(6, 2);
				if (("**" != tempString))
				{
					day = int.Parse(tempString);
				}
				tempString = dmtf.Substring(8, 2);
				if (("**" != tempString))
				{
					hour = int.Parse(tempString);
				}
				tempString = dmtf.Substring(10, 2);
				if (("**" != tempString))
				{
					minute = int.Parse(tempString);
				}
				tempString = dmtf.Substring(12, 2);
				if (("**" != tempString))
				{
					second = int.Parse(tempString);
				}
				tempString = dmtf.Substring(15, 6);
				if (("******" != tempString))
				{
					ticks = long.Parse(tempString) * (TimeSpan.TicksPerMillisecond / 1000);
				}
				if (((((((((year < 0)
							|| (month < 0))
							|| (day < 0))
							|| (hour < 0))
							|| (minute < 0))
							|| (minute < 0))
							|| (second < 0))
							|| (ticks < 0)))
				{
					throw new ArgumentOutOfRangeException(nameof(dmtfDate));
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				throw new ArgumentOutOfRangeException(null, e.Message);
			}
			datetime = new DateTime(year, month, day, hour, minute, second, 0);
			datetime = datetime.AddTicks(ticks);
			TimeSpan tickOffset = TimeZoneInfo.Local.GetUtcOffset(datetime);
			int uTCOffset = 0;
			int offsetToBeAdjusted = 0;
			long offsetMins = tickOffset.Ticks / TimeSpan.TicksPerMinute;
			tempString = dmtf.Substring(22, 3);
			if ((tempString != "******"))
			{
				tempString = dmtf.Substring(21, 4);
				try
				{
					uTCOffset = int.Parse(tempString);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					throw new ArgumentOutOfRangeException(null, e.Message);
				}
				offsetToBeAdjusted = ((int)((offsetMins - uTCOffset)));
				datetime = datetime.AddMinutes(offsetToBeAdjusted);
			}
			return datetime;
		}

		bool ShouldSerializeInstallDate()
		{
			if ((!this.IsInstallDateNull))
			{
				return true;
			}
			return false;
		}

		bool ShouldSerializeProcessId()
		{
			if ((!this.IsProcessIdNull))
			{
				return true;
			}
			return false;
		}

		bool ShouldSerializeServiceSpecificExitCode()
		{
			if ((!this.IsServiceSpecificExitCodeNull))
			{
				return true;
			}
			return false;
		}

		bool ShouldSerializeStarted()
		{
			if ((!this.IsStartedNull))
			{
				return true;
			}
			return false;
		}

		bool ShouldSerializeTagId()
		{
			if ((!this.IsTagIdNull))
			{
				return true;
			}
			return false;
		}

		bool ShouldSerializeWaitHint()
		{
			if ((!this.IsWaitHintNull))
			{
				return true;
			}
			return false;
		}

		[Browsable(true)]
		public void CommitObject()
		{
			if ((!isEmbedded))
			{
				PrivateLateBoundObject.Put();
			}
		}

		[Browsable(true)]
		public void CommitObject(PutOptions putOptions)
		{
			if ((!isEmbedded))
			{
				PrivateLateBoundObject.Put(putOptions);
			}
		}

		void Initialize()
		{
			AutoCommitProp = true;
			isEmbedded = false;
		}

		static string ConstructPath(string keyName)
		{
			string strPath = "root\\CimV2:Win32_Service";
			strPath = string.Concat(strPath, string.Concat(".Name=", string.Concat("\"", string.Concat(keyName, "\""))));
			return strPath;
		}

		void InitializeObject(ManagementScope mgmtScope, ManagementPath path, ObjectGetOptions getOptions)
		{
			Initialize();
			if ((path != null))
			{
				if ((!CheckIfProperClass(mgmtScope, path, getOptions)))
				{
					throw new ArgumentException("Class name does not match.");
				}
			}
			PrivateLateBoundObject = new ManagementObject(mgmtScope, path, getOptions);
			PrivateSystemProperties = new ManagementSystemProperties(PrivateLateBoundObject);
			curObj = PrivateLateBoundObject;
		}

		// Different overloads of GetInstances() help in enumerating instances of the WMI class.
		public static ServiceCollection GetInstances()
		{
			return GetInstances(null, null, null);
		}

		public static ServiceCollection GetInstances(string condition)
		{
			return GetInstances(null, condition, null);
		}

		public static ServiceCollection GetInstances(System.String[] selectedProperties)
		{
			return GetInstances(null, null, selectedProperties);
		}

		public static ServiceCollection GetInstances(string condition, System.String[] selectedProperties)
		{
			return GetInstances(null, condition, selectedProperties);
		}

		public static ServiceCollection GetInstances(ManagementScope mgmtScope, EnumerationOptions enumOptions)
		{
			if ((mgmtScope == null))
			{
				if ((statMgmtScope == null))
				{
					mgmtScope = new ManagementScope();
					mgmtScope.Path.NamespacePath = "root\\CimV2";
				}
				else
				{
					mgmtScope = statMgmtScope;
				}
			}
			ManagementPath pathObj = new ManagementPath();
			pathObj.ClassName = "Win32_Service";
			pathObj.NamespacePath = "root\\CimV2";
			ManagementClass clsObject = new ManagementClass(mgmtScope, pathObj, null);
			if ((enumOptions == null))
			{
				enumOptions = new EnumerationOptions();
				enumOptions.EnsureLocatable = true;
			}
			return new ServiceCollection(clsObject.GetInstances(enumOptions));
		}

		public static ServiceCollection GetInstances(ManagementScope mgmtScope, string condition)
		{
			return GetInstances(mgmtScope, condition, null);
		}

		public static ServiceCollection GetInstances(ManagementScope mgmtScope, System.String[] selectedProperties)
		{
			return GetInstances(mgmtScope, null, selectedProperties);
		}

		public static ServiceCollection GetInstances(ManagementScope mgmtScope, string condition, System.String[] selectedProperties)
		{
			if ((mgmtScope == null))
			{
				if ((statMgmtScope == null))
				{
					mgmtScope = new ManagementScope();
					mgmtScope.Path.NamespacePath = "root\\CimV2";
				}
				else
				{
					mgmtScope = statMgmtScope;
				}
			}
			ManagementObjectSearcher objectSearcher = new ManagementObjectSearcher(mgmtScope, new SelectQuery("Win32_Service", condition, selectedProperties));
			EnumerationOptions enumOptions = new EnumerationOptions();
			enumOptions.EnsureLocatable = true;
			objectSearcher.Options = enumOptions;
			return new ServiceCollection(objectSearcher.Get());
		}

		[Browsable(true)]
		public static Service CreateInstance()
		{
			ManagementScope mgmtScope = null;
			if ((statMgmtScope == null))
			{
				mgmtScope = new ManagementScope();
				mgmtScope.Path.NamespacePath = CreatedWmiNamespace;
			}
			else
			{
				mgmtScope = statMgmtScope;
			}
			ManagementPath mgmtPath = new ManagementPath(CreatedClassName);
			ManagementClass tmpMgmtClass = new ManagementClass(mgmtScope, mgmtPath, null);
			return new Service(tmpMgmtClass.CreateInstance());
		}

		[Browsable(true)]
		public void Delete()
		{
			PrivateLateBoundObject.Delete();
		}

		public uint Change(bool desktopInteract, string displayName, byte errorControl, string loadOrderGroup, string[] loadOrderGroupDependencies, string pathName, string[] serviceDependencies, byte serviceType, string startMode, string startName, string startPassword)
		{
			if ((!isEmbedded))
			{
				ManagementBaseObject inParams = null;
				inParams = PrivateLateBoundObject.GetMethodParameters("Change");
				inParams[nameof(DesktopInteract)] = desktopInteract;
				inParams[nameof(DisplayName)] = displayName;
				inParams[nameof(ErrorControl)] = errorControl;
				inParams["LoadOrderGroup"] = loadOrderGroup;
				inParams["LoadOrderGroupDependencies"] = loadOrderGroupDependencies;
				inParams[nameof(PathName)] = pathName;
				inParams["ServiceDependencies"] = serviceDependencies;
				inParams[nameof(ServiceType)] = serviceType;
				inParams[nameof(StartMode)] = startMode;
				inParams[nameof(StartName)] = startName;
				inParams["StartPassword"] = startPassword;
				ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("Change", inParams, null);
				return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
			}
			else
			{
				return Convert.ToUInt32(0);
			}
		}

		public uint ChangeStartMode(string startMode)
		{
			if ((!isEmbedded))
			{
				ManagementBaseObject inParams = null;
				inParams = PrivateLateBoundObject.GetMethodParameters("ChangeStartMode");
				inParams[nameof(StartMode)] = startMode;
				ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("ChangeStartMode", inParams, null);
				return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
			}
			else
			{
				return Convert.ToUInt32(0);
			}
		}

		public uint Create(bool desktopInteract, string displayName, byte errorControl, string loadOrderGroup, string[] loadOrderGroupDependencies, string name, string pathName, string[] serviceDependencies, byte serviceType, string startMode, string startName, string startPassword)
		{
			if ((!isEmbedded))
			{
				ManagementBaseObject inParams = null;
				inParams = PrivateLateBoundObject.GetMethodParameters("Create");
				inParams[nameof(DesktopInteract)] = desktopInteract;
				inParams[nameof(DisplayName)] = displayName;
				inParams[nameof(ErrorControl)] = errorControl;
				inParams["LoadOrderGroup"] = loadOrderGroup;
				inParams["LoadOrderGroupDependencies"] = loadOrderGroupDependencies;
				inParams[nameof(Name)] = name;
				inParams[nameof(PathName)] = pathName;
				inParams["ServiceDependencies"] = serviceDependencies;
				inParams[nameof(ServiceType)] = serviceType;
				inParams[nameof(StartMode)] = startMode;
				inParams[nameof(StartName)] = startName;
				inParams["StartPassword"] = startPassword;
				ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("Create", inParams, null);
				return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
			}
			else
			{
				return Convert.ToUInt32(0);
			}
		}

		public uint Delete0()
		{
			if ((!isEmbedded))
			{
				ManagementBaseObject inParams = null;
				ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("Delete", inParams, null);
				return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
			}
			else
			{
				return Convert.ToUInt32(0);
			}
		}

		public uint InterrogateService()
		{
			if ((!isEmbedded))
			{
				ManagementBaseObject inParams = null;
				ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("InterrogateService", inParams, null);
				return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
			}
			else
			{
				return Convert.ToUInt32(0);
			}
		}

		public uint PauseService()
		{
			if ((!isEmbedded))
			{
				ManagementBaseObject inParams = null;
				ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("PauseService", inParams, null);
				return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
			}
			else
			{
				return Convert.ToUInt32(0);
			}
		}

		public uint ResumeService()
		{
			if ((!isEmbedded))
			{
				ManagementBaseObject inParams = null;
				ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("ResumeService", inParams, null);
				return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
			}
			else
			{
				return Convert.ToUInt32(0);
			}
		}

		public uint StartService()
		{
			if ((!isEmbedded))
			{
				ManagementBaseObject inParams = null;
				ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("StartService", inParams, null);
				return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
			}
			else
			{
				return Convert.ToUInt32(0);
			}
		}

		public uint StopService()
		{
			if ((!isEmbedded))
			{
				ManagementBaseObject inParams = null;
				ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("StopService", inParams, null);
				return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
			}
			else
			{
				return Convert.ToUInt32(0);
			}
		}

		public uint UserControlService(byte controlCode)
		{
			if ((!isEmbedded))
			{
				ManagementBaseObject inParams = null;
				inParams = PrivateLateBoundObject.GetMethodParameters("UserControlService");
				inParams["ControlCode"] = controlCode;
				ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("UserControlService", inParams, null);
				return Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
			}
			else
			{
				return Convert.ToUInt32(0);
			}
		}

		// Enumerator implementation for enumerating instances of the class.
		public class ServiceCollection : object, ICollection
		{
			readonly ManagementObjectCollection privColObj;

			public ServiceCollection(ManagementObjectCollection objCollection)
			{
				privColObj = objCollection;
			}

			public virtual int Count
			{
				get
				{
					return privColObj.Count;
				}
			}

			public virtual bool IsSynchronized
			{
				get
				{
					return privColObj.IsSynchronized;
				}
			}

			public virtual object SyncRoot
			{
				get
				{
					return this;
				}
			}

			public virtual void CopyTo(Array array, int index)
			{
				privColObj.CopyTo(array, index);
				int nCtr;
				for (nCtr = 0; (nCtr < array.Length); nCtr = (nCtr + 1))
				{
					array.SetValue(new Service(((ManagementObject)(array.GetValue(nCtr)))), nCtr);
				}
			}

			public virtual IEnumerator GetEnumerator()
			{
				return new ServiceEnumerator(privColObj.GetEnumerator());
			}

			public class ServiceEnumerator : object, IEnumerator
			{
				readonly ManagementObjectCollection.ManagementObjectEnumerator privObjEnum;

				public ServiceEnumerator(ManagementObjectCollection.ManagementObjectEnumerator objEnum)
				{
					privObjEnum = objEnum;
				}

				public virtual object Current
				{
					get
					{
						return new Service(((ManagementObject)(privObjEnum.Current)));
					}
				}

				public virtual bool MoveNext()
				{
					return privObjEnum.MoveNext();
				}

				public virtual void Reset()
				{
					privObjEnum.Reset();
				}
			}
		}

		// TypeConverter to handle null values for ValueType properties
		public class WMIValueTypeConverter : TypeConverter
		{
			readonly TypeConverter baseConverter;

			readonly Type baseType;

			public WMIValueTypeConverter(Type inBaseType)
			{
				baseConverter = TypeDescriptor.GetConverter(inBaseType);
				baseType = inBaseType;
			}

			public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
			{
				return baseConverter.CanConvertFrom(context, srcType);
			}

			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			{
				return baseConverter.CanConvertTo(context, destinationType);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
			{
				return baseConverter.ConvertFrom(context, culture, value);
			}

			public override object CreateInstance(ITypeDescriptorContext context, IDictionary dictionary)
			{
				return baseConverter.CreateInstance(context, dictionary);
			}

			public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
			{
				return baseConverter.GetCreateInstanceSupported(context);
			}

			public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributeVar)
			{
				return baseConverter.GetProperties(context, value, attributeVar);
			}

			public override bool GetPropertiesSupported(ITypeDescriptorContext context)
			{
				return baseConverter.GetPropertiesSupported(context);
			}

			public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				return baseConverter.GetStandardValues(context);
			}

			public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			{
				return baseConverter.GetStandardValuesExclusive(context);
			}

			public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			{
				return baseConverter.GetStandardValuesSupported(context);
			}

			public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
			{
				if ((baseType.BaseType == typeof(Enum)))
				{
					if ((value.GetType() == destinationType))
					{
						return value;
					}
					if ((((value == null)
								&& (context != null))
								&& (!context.PropertyDescriptor.ShouldSerializeValue(context.Instance))))
					{
						return "NULL_ENUM_VALUE";
					}
					return baseConverter.ConvertTo(context, culture, value, destinationType);
				}
				if (((baseType == typeof(bool))
							&& (baseType.BaseType == typeof(ValueType))))
				{
					if ((((value == null)
								&& (context != null))
								&& (!context.PropertyDescriptor.ShouldSerializeValue(context.Instance))))
					{
						return "";
					}
					return baseConverter.ConvertTo(context, culture, value, destinationType);
				}
				if (((context != null)
							&& (!context.PropertyDescriptor.ShouldSerializeValue(context.Instance))))
				{
					return "";
				}
				return baseConverter.ConvertTo(context, culture, value, destinationType);
			}
		}

		// Embedded class to represent WMI system Properties.
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class ManagementSystemProperties
		{
			readonly ManagementBaseObject PrivateLateBoundObject;

			public ManagementSystemProperties(ManagementBaseObject managedObject)
			{
				PrivateLateBoundObject = managedObject;
			}

			[Browsable(true)]
			public int GENUS
			{
				get
				{
					return ((int)(PrivateLateBoundObject["__GENUS"]));
				}
			}

			[Browsable(true)]
			public string CLASS
			{
				get
				{
					return ((string)(PrivateLateBoundObject["__CLASS"]));
				}
			}

			[Browsable(true)]
			public string SUPERCLASS
			{
				get
				{
					return ((string)(PrivateLateBoundObject["__SUPERCLASS"]));
				}
			}

			[Browsable(true)]
			public string DYNASTY
			{
				get
				{
					return ((string)(PrivateLateBoundObject["__DYNASTY"]));
				}
			}

			[Browsable(true)]
			public string RELPATH
			{
				get
				{
					return ((string)(PrivateLateBoundObject["__RELPATH"]));
				}
			}

			[Browsable(true)]
			public int PROPERTY_COUNT
			{
				get
				{
					return ((int)(PrivateLateBoundObject["__PROPERTY_COUNT"]));
				}
			}

			[Browsable(true)]
			public string[] DERIVATION
			{
				get
				{
					return ((string[])(PrivateLateBoundObject["__DERIVATION"]));
				}
			}

			[Browsable(true)]
			public string SERVER
			{
				get
				{
					return ((string)(PrivateLateBoundObject["__SERVER"]));
				}
			}

			[Browsable(true)]
			public string NAMESPACE
			{
				get
				{
					return ((string)(PrivateLateBoundObject["__NAMESPACE"]));
				}
			}

			[Browsable(true)]
			public string PATH
			{
				get
				{
					return ((string)(PrivateLateBoundObject["__PATH"]));
				}
			}
		}
	}
}
