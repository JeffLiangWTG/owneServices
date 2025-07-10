=== Overview ===
ActivityLogger is a program that logs mouse, keyboard and CBT hooks/events from windows. To do this it uses native windows hooks and dll injection. This is
performed in the C++ dll called Syshook. The results are then polled in a loop in the c# code.
- C#/C++ interop is achieved with pInvoke.
- AnyCPU is achieved by C# code checking platform and invoking the right C++ DLL.
- Injected dll hooks send data back to the main process via IPC/Shared Memory.
- The ActivityLogger C++ dll is a singleton process and uses OS mutexes to ensure only one of itself is running at a given time.
- The C++ dll stores hook events in a circular buffer and the C# application polls the C++ code for new messages, which the C++ dll returns one by one.
- Received hook messages are buffered and saved on a timer, whose timing can be found in
Enterprise/Architecture/GUI/Forms/ActivityLogger/ZFormActivityLogger.cs. Changing these timings is very useful for debugging since you get results quicker.
- ActivityLogger (C#) is built in both Debug and Release modes. Both modes use the Debug build of the C++ libs, which have no Release build.



=== How To Build ===
=== C++
1. Rename the ActivityLoggerC++.txt to ActivityLoggerC++.sln. This is to trick able broken reflection test that fails if it thinks solutions are unused.
Truthfully, the C++ solution isn't used in the QGL build process but this is only because QGL does not work with C++ solutions.
2. Open ActivityLoggerC++.sln and build the Syshook project for both x86 and x64. This can be done with the "Batch Build" command (Build -> Batch build) or by
compiling them individually one at a time.
3. There is a post-build event that copies both x86 and x64 binaries into the 'prebuilt' folder.
4. These platform-specific binaries are copied during QGL by Build.xml config into bin/x86 and bin/x64. In other words, if you want to make changes to the
C++/Syshook library and see/test them, you need to either run QGL to copy the binaries to the bin folder or manually copy them across.

== C#
1. Open ActivityLogger.sln and build it. It should just workâ„¢.
Note: there is a prebuild step that copies the Syshook binaries from the prebuilt folder to the Syshook binary folder, for local testing. If that fails, check
the C++ binaries are built (both x86/x64) and in the correct location.



=== Technical Notes/Limitations ===
- Due to the way global hooks work in windows, it is not possible for C# to read any global hooks (except LL_KEYBOARD and LL_MOUSE). This is due to lack of
support for global hooking in the CLR and isn't something we can fix.
- Windows can only inject a 64-bit dll into a 64-bit process and a 32-bit dll into a 32-bit process. As such, if running CW1 in 32-bit mode the activity logger
will only be able to hook and track 32-bit processes, and similarly for 64-bit CW1, only 64-bit processes can be hooked.
- The ActivityLogger spins up a thread to poll the Syshook library for message. This thread contains a while(true) loop and a thread-local instance of Hooker,
the class that adds RAII-style management to windows hooks for this project.



=== Future Work ===
- Rewrite IPC/Shared Memory implementation so a normal person can understand it.
- Include the platform (x86/x64) in the log. I've done a dirty implementation by appending the platform to the process name but this isn't too useful since we
can't sort by it. For that we need to add another column for this in the DB, but that's out of scope for this WI (WI00208692).
- Cleanup the C# code and implement all the modern C# features (ie use var, auto properties, etc).
- Clean up the threading model and the use of static/volatile/overridable variables.
- Fix the QGL bug that doesn't allow C++ projects to be in the same solution as C# projects - then we can remove the ActivityLoggerC++.sln solution and have a
unified build process in a single solution.
